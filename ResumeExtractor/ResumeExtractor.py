# import re
# import os
# import io
# import spacy
# import docx
# import PyPDF2
# import pytesseract
# import dateparser
# from PIL import Image
# from fuzzywuzzy import fuzz

# # Load SpaCy English model
# nlp = spacy.load("en_core_web_sm")

# def extract_text_from_pdf(file_stream):
#     """Extract text from PDF, including OCR for scanned docs."""
#     text = ""
#     pdf_reader = PyPDF2.PdfReader(file_stream)
#     for page in pdf_reader.pages:
#         if page.extract_text():
#             text += page.extract_text()
#         else:
#             # OCR for scanned pages
#             xObject = page.get("/Resources").get("/XObject")
#             if xObject:
#                 for obj in xObject:
#                     if xObject[obj]["/Subtype"] == "/Image":
#                         size = (xObject[obj]["/Width"], xObject[obj]["/Height"])
#                         data = xObject[obj].get_data()
#                         img = Image.frombytes("RGB", size, data)
#                         text += pytesseract.image_to_string(img)
#     return text

# def extract_text_from_docx(file_stream):
#     """Extract text from DOCX."""
#     doc = docx.Document(file_stream)
#     return "\n".join([para.text for para in doc.paragraphs])

# def extract_contact_info(text):
#     """Extract name, email, and phone number."""
#     email = re.search(r"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", text)
#     phone = re.search(r"\+?\d[\d\-\(\) ]{8,}\d", text)
    
#     doc = nlp(text)
#     name = ""
#     for ent in doc.ents:
#         if ent.label_ == "PERSON":
#             name = ent.text
#             break
#     return {
#         "name": name.strip() if name else None,
#         "email": email.group(0) if email else None,
#         "phone": phone.group(0) if phone else None
#     }

# def extract_skills(text, skills_list):
#     """Fuzzy match skills from given list."""
#     found_skills = []
#     for skill in skills_list:
#         if fuzz.partial_ratio(skill.lower(), text.lower()) > 80:
#             found_skills.append(skill)
#     return list(set(found_skills))

# def extract_education(text):
#     """Extract education details."""
#     education = []
#     edu_pattern = r"(B\.?Tech|M\.?Tech|B\.?Sc|M\.?Sc|Bachelor|Master|Ph\.?D).*"
#     for line in text.split("\n"):
#         if re.search(edu_pattern, line, re.IGNORECASE):
#             dates = re.findall(r"\b(19|20)\d{2}\b", line)
#             education.append({
#                 "degree": line.strip(),
#                 "start_date": dates[0] if len(dates) > 0 else None,
#                 "end_date": dates[1] if len(dates) > 1 else None
#             })
#     return education

# def extract_experience(text):
#     """Extract work experience."""
#     experience = []
#     exp_pattern = r"(Intern|Engineer|Developer|Manager|Consultant|Designer|Lead)"
#     for line in text.split("\n"):
#         if re.search(exp_pattern, line, re.IGNORECASE):
#             dates = re.findall(r"\b(19|20)\d{2}\b", line)
#             experience.append({
#                 "job_title": line.strip(),
#                 "start_date": dates[0] if len(dates) > 0 else None,
#                 "end_date": dates[1] if len(dates) > 1 else None
#             })
#     return experience

# def extract_certifications(text):
#     """Extract certifications."""
#     cert_keywords = ["certified", "certification", "certificate", "AWS", "Azure", "Google Cloud"]
#     certs = []
#     for line in text.split("\n"):
#         if any(keyword.lower() in line.lower() for keyword in cert_keywords):
#             certs.append(line.strip())
#     return certs

# def parse_resume(file_stream, filename, skills_list):
#     """Main resume parsing function."""
#     ext = os.path.splitext(filename)[1].lower()
#     if ext == ".pdf":
#         text = extract_text_from_pdf(file_stream)
#     elif ext == ".docx":
#         text = extract_text_from_docx(file_stream)
#     else:
#         raise ValueError("Unsupported file format")

#     contact = extract_contact_info(text)
#     skills = extract_skills(text, skills_list)
#     education = extract_education(text)
#     experience = extract_experience(text)
#     certifications = extract_certifications(text)

#     return {
#         "name": contact["name"],
#         "email": contact["email"],
#         "phone": contact["phone"],
#         "skills": skills,
#         "education": education,
#         "experience": experience,
#         "certifications": certifications
#     }
