from fastapi import FastAPI, File, UploadFile, Form
import spacy
import pdfplumber
import docx
import chardet
import re
import json
from io import BytesIO

app = FastAPI()
nlp = spacy.load("en_core_web_sm")

def extract_text_from_file(file: UploadFile) -> str:
    extension = file.filename.split(".")[-1].lower()
    content = file.file.read()

    if extension == "pdf":
        with pdfplumber.open(BytesIO(content)) as pdf:
            return "\n".join(page.extract_text() or '' for page in pdf.pages)

    elif extension == "docx":
        doc = docx.Document(BytesIO(content))
        return "\n".join([para.text for para in doc.paragraphs])

    elif extension == "txt":
        encoding = chardet.detect(content)['encoding']
        return content.decode(encoding or 'utf-8')

    else:
        return ""


def extract_skills(text: str, master_skills: set):
    lower_text = text.lower()
    return sorted({skill for skill in master_skills if skill in lower_text})


def extract_experience(text: str) -> float:
    patterns = [
        r"(\d+\.?\d*)\s*(?:\+?\s*)?(?:years|yrs|year|yr)\b",
        r"experience\s*[:-]?\s*(\d+\.?\d*)",
        r"(\d+)\s*to\s*(\d+)\s*(?:years|yrs|year|yr)\b",
        r"(\d+)\s*-\s*(\d+)\s*(?:years|yrs|year|yr)\b"
    ]

    all_matches = []
    for pattern in patterns:
        matches = re.findall(pattern, text.lower())
        for match in matches:
            if isinstance(match, tuple):
                avg_exp = (float(match[0]) + float(match[1])) / 2
                all_matches.append(avg_exp)
            else:
                all_matches.append(float(match))

    return max(all_matches, default=0.0)


def calculate_match(resume_skills, jd_skills, resume_exp, jd_exp) -> dict:
    matched_skills = list(set(resume_skills) & set(jd_skills))
    missing_skills = list(set(jd_skills) - set(resume_skills))

    skill_match = len(matched_skills)
    total_skills = len(set(jd_skills))
    skill_score = (skill_match / total_skills * 100) if total_skills > 0 else 0

    exp_score = 100 if abs(resume_exp - jd_exp) <= 0.5 else 75 if abs(resume_exp - jd_exp) <= 1 else 50
    final_score = round(0.7 * skill_score + 0.3 * exp_score, 2)

    return {
        "match_score": final_score,
        "skill_match": skill_score,
        "experience_score": exp_score,
        "matched_skills": matched_skills,
        "missing_skills": missing_skills
    }


@app.post("/analyze-resume")
async def analyze_resume(
    file: UploadFile = File(...),
    job_description: str = Form(...),
    master_skills_json: str = Form(...)
):
    master_skills = set(json.loads(master_skills_json))

    resume_text = extract_text_from_file(file)

    resume_skills = extract_skills(resume_text, master_skills)
    jd_skills = extract_skills(job_description, master_skills)
    resume_experience = extract_experience(resume_text)
    jd_experience = extract_experience(job_description)

    if jd_experience == 0.0:
        jd_experience = resume_experience

    scores = calculate_match(resume_skills, jd_skills, resume_experience, jd_experience)

    return {
        "filename": file.filename,
        "resume_skills": resume_skills,
        "jd_skills": jd_skills,
        "resume_experience_years": resume_experience,
        "jd_expected_experience": jd_experience,
        "score_details": scores
    }
