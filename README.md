HireFlow

An AI-powered recruitment and onboarding platform built using ASP.NET Core 8, MVC, Entity Framework Core, AWS Textract/Comprehend, and OpenAI GPT.

Overview

HireFlow modernizes the hiring lifecycle by automating job postings, resume extraction, candidate evaluation, interview management, and onboarding workflows. The system converts unstructured resumes into structured profiles, performs skill and experience analysis using AWS NLP services, and applies GPT-based scoring to generate accurate suitability evaluations. HR teams gain a unified platform for managing applications, tracking pipeline stages, and generating AI-assisted job descriptions and interview questions.

Key Features

AI-driven resume parsing using AWS Textract and Comprehend

GPT-powered scoring for skills, experience, resume relevance, and overall fit

Job posting with AI-generated descriptions

Candidate profile creation, document uploads, and application tracking

Multi-round interview scheduling with integrated feedback

Role-based authentication with Identity + JWT and OTP verification via Amazon SNS

Centralized dashboards for HR, recruiters, and candidates

Architecture

HireFlow follows a clean, layered architecture with separation between Controllers, Services, and Repositories, ensuring maintainability and scalability. The backend exposes REST APIs, while the MVC frontend handles presentation and workflow interactions. SQL Server stores the normalized relational dataset, and all external integrations (Textract, Comprehend, GPT, SNS) are encapsulated in dedicated service components. Security is enforced through token-based authentication, encrypted credentials, and validated file handling.

Technology Stack

Backend: ASP.NET Core 8 Web API, EF Core, LINQ

Frontend: ASP.NET MVC, Bootstrap 5, jQuery

Cloud & AI: AWS Textract, AWS Comprehend, AWS SNS, OpenAI GPT

Database: SQL Server 2022

Tooling: Swagger, IIS, Visual Studio 2022

Deployment

The platform supports cloud-ready deployment models using IIS and AWS integrations. Secrets, API keys, and cloud credentials are managed securely. Logs, resume extractions, and scoring routines run in a distributed workflow leveraging Textract, Comprehend, and GPT services.

Testing

Unit, integration, and UAT phases validate the correctness of the API workflows, cloud integrations, authentication mechanisms, and HR processes. Special attention is given to data integrity, error handling, and API reliability under real recruitment workloads.

Future Enhancements

Predictive hiring analytics

AI-based video interview evaluation

Automated offer letter generation

Multi-platform job posting (LinkedIn, Naukri, Indeed)

Fraud detection for resume and document verification

HRMS and payroll system integrations
