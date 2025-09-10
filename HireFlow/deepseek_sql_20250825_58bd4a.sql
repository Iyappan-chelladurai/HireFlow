-- ModernTech Recruitment Database Schema
-- Created based on analysis of HTML files

-- Enable UUID extension if using PostgreSQL
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Companies table
CREATE TABLE companies (
    company_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    logo_url VARCHAR(500),
    website_url VARCHAR(500),
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Departments table
CREATE TABLE departments (
    department_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Users/Employees table (for recruiters, hiring managers, etc.)
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255),
    role VARCHAR(50) NOT NULL, -- recruiter, hiring_manager, admin, etc.
    department_id INTEGER REFERENCES departments(department_id),
    profile_picture_url VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Job Positions table
CREATE TABLE positions (
    position_id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    department_id INTEGER REFERENCES departments(department_id),
    description TEXT,
    requirements TEXT,
    responsibilities TEXT,
    experience_level VARCHAR(50), -- intern, entry, mid, senior, director
    education_level VARCHAR(50), -- highschool, associate, bachelor, master, phd
    employment_type VARCHAR(50), -- fulltime, parttime, contract, internship
    location VARCHAR(255),
    is_remote BOOLEAN DEFAULT FALSE,
    salary_range_min DECIMAL(10, 2),
    salary_range_max DECIMAL(10, 2),
    status VARCHAR(20) DEFAULT 'draft', -- draft, active, closed
    created_by INTEGER REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Job Benefits table
CREATE TABLE position_benefits (
    benefit_id SERIAL PRIMARY KEY,
    position_id INTEGER REFERENCES positions(position_id) ON DELETE CASCADE,
    benefit VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Required Skills table
CREATE TABLE position_skills (
    skill_id SERIAL PRIMARY KEY,
    position_id INTEGER REFERENCES positions(position_id) ON DELETE CASCADE,
    skill VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Candidates table
CREATE TABLE candidates (
    candidate_id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    phone VARCHAR(50),
    resume_url VARCHAR(500),
    cover_letter TEXT,
    profile_picture_url VARCHAR(500),
    location VARCHAR(255),
    experience_years INTEGER,
    education_level VARCHAR(50),
    source VARCHAR(100), -- LinkedIn, Company Website, Referrals, Job Boards, etc.
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Candidate Skills table
CREATE TABLE candidate_skills (
    candidate_skill_id SERIAL PRIMARY KEY,
    candidate_id INTEGER REFERENCES candidates(candidate_id) ON DELETE CASCADE,
    skill VARCHAR(100) NOT NULL,
    proficiency_level VARCHAR(50), -- beginner, intermediate, advanced, expert
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Applications table
CREATE TABLE applications (
    application_id SERIAL PRIMARY KEY,
    candidate_id INTEGER REFERENCES candidates(candidate_id),
    position_id INTEGER REFERENCES positions(position_id),
    application_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) DEFAULT 'applied', -- applied, viewed, screening, interview, offered, hired, rejected
    match_score INTEGER, -- percentage match based on skills/requirements
    assigned_recruiter_id INTEGER REFERENCES users(user_id),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(candidate_id, position_id)
);

-- Application Status History table
CREATE TABLE application_status_history (
    history_id SERIAL PRIMARY KEY,
    application_id INTEGER REFERENCES applications(application_id) ON DELETE CASCADE,
    status VARCHAR(50) NOT NULL,
    changed_by INTEGER REFERENCES users(user_id),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Interviews table
CREATE TABLE interviews (
    interview_id SERIAL PRIMARY KEY,
    application_id INTEGER REFERENCES applications(application_id),
    interview_type VARCHAR(100), -- technical, culture, hr, manager, etc.
    interview_date TIMESTAMP NOT NULL,
    duration_minutes INTEGER,
    interviewer_id INTEGER REFERENCES users(user_id),
    location VARCHAR(500), -- physical location or video conference link
    status VARCHAR(50) DEFAULT 'scheduled', -- scheduled, completed, cancelled, rescheduled
    notes TEXT,
    feedback TEXT,
    rating INTEGER, -- 1-5 scale
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Interview Participants table (for multiple interviewers)
CREATE TABLE interview_participants (
    participant_id SERIAL PRIMARY KEY,
    interview_id INTEGER REFERENCES interviews(interview_id) ON DELETE CASCADE,
    user_id INTEGER REFERENCES users(user_id),
    role VARCHAR(100), -- interviewer, observer, etc.
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Offers table
CREATE TABLE offers (
    offer_id SERIAL PRIMARY KEY,
    application_id INTEGER REFERENCES applications(application_id),
    position_id INTEGER REFERENCES positions(position_id),
    candidate_id INTEGER REFERENCES candidates(candidate_id),
    base_salary DECIMAL(10, 2) NOT NULL,
    signing_bonus DECIMAL(10, 2) DEFAULT 0,
    annual_bonus_target DECIMAL(5, 2), -- percentage
    stock_options INTEGER,
    benefits TEXT, -- JSON or comma-separated list of benefits
    start_date DATE,
    decision_deadline DATE,
    status VARCHAR(50) DEFAULT 'pending', -- pending, accepted, rejected, expired
    offer_letter_url VARCHAR(500),
    created_by INTEGER REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Onboarding table
CREATE TABLE onboarding (
    onboarding_id SERIAL PRIMARY KEY,
    offer_id INTEGER REFERENCES offers(offer_id),
    candidate_id INTEGER REFERENCES candidates(candidate_id),
    position_id INTEGER REFERENCES positions(position_id),
    start_date DATE NOT NULL,
    status VARCHAR(50) DEFAULT 'pending', -- pending, in_progress, completed
    progress_percentage INTEGER DEFAULT 0,
    assigned_hr_id INTEGER REFERENCES users(user_id),
    assigned_manager_id INTEGER REFERENCES users(user_id),
    mentor_id INTEGER REFERENCES users(user_id),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Onboarding Stages table
CREATE TABLE onboarding_stages (
    stage_id SERIAL PRIMARY KEY,
    onboarding_id INTEGER REFERENCES onboarding(onboarding_id) ON DELETE CASCADE,
    stage_name VARCHAR(100) NOT NULL,
    stage_order INTEGER NOT NULL,
    due_date DATE,
    completed_date TIMESTAMP,
    status VARCHAR(50) DEFAULT 'pending', -- pending, in_progress, completed
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Onboarding Tasks table
CREATE TABLE onboarding_tasks (
    task_id SERIAL PRIMARY KEY,
    onboarding_id INTEGER REFERENCES onboarding(onboarding_id) ON DELETE CASCADE,
    stage_id INTEGER REFERENCES onboarding_stages(stage_id),
    task_name VARCHAR(255) NOT NULL,
    description TEXT,
    assigned_to INTEGER REFERENCES users(user_id),
    due_date DATE,
    completed_date TIMESTAMP,
    status VARCHAR(50) DEFAULT 'pending', -- pending, in_progress, completed, overdue
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Onboarding Documents table
CREATE TABLE onboarding_documents (
    document_id SERIAL PRIMARY KEY,
    onboarding_id INTEGER REFERENCES onboarding(onboarding_id) ON DELETE CASCADE,
    document_name VARCHAR(255) NOT NULL,
    document_type VARCHAR(100), -- contract, NDA, tax forms, etc.
    document_url VARCHAR(500),
    status VARCHAR(50) DEFAULT 'pending', -- pending, submitted, verified, rejected
    submitted_date TIMESTAMP,
    verified_date TIMESTAMP,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Reports table (for storing report configurations and data)
CREATE TABLE reports (
    report_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    report_type VARCHAR(100), -- recruitment_overview, time_to_hire, source_effectiveness, etc.
    filters JSON, -- store filter configurations as JSON
    created_by INTEGER REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Recruitment Metrics table (for storing historical metrics data)
CREATE TABLE recruitment_metrics (
    metric_id SERIAL PRIMARY KEY,
    period DATE NOT NULL, -- month or quarter this metric represents
    department_id INTEGER REFERENCES departments(department_id),
    open_positions INTEGER DEFAULT 0,
    applications_received INTEGER DEFAULT 0,
    interviews_conducted INTEGER DEFAULT 0,
    offers_made INTEGER DEFAULT 0,
    hires INTEGER DEFAULT 0,
    time_to_fill_days INTEGER, -- average time to fill positions
    cost_per_hire DECIMAL(10, 2),
    offer_acceptance_rate DECIMAL(5, 2), -- percentage
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for better performance
CREATE INDEX idx_applications_candidate ON applications(candidate_id);
CREATE INDEX idx_applications_position ON applications(position_id);
CREATE INDEX idx_applications_status ON applications(status);
CREATE INDEX idx_interviews_date ON interviews(interview_date);
CREATE INDEX idx_interviews_application ON interviews(application_id);
CREATE INDEX idx_offers_application ON offers(application_id);
CREATE INDEX idx_onboarding_candidate ON onboarding(candidate_id);
CREATE INDEX idx_onboarding_status ON onboarding(status);
CREATE INDEX idx_positions_status ON positions(status);
CREATE INDEX idx_positions_department ON positions(department_id);

-- Insert default departments
INSERT INTO departments (name, description) VALUES
('Engineering', 'Software development and engineering department'),
('Design', 'Product design and user experience department'),
('Product', 'Product management and strategy department'),
('Marketing', 'Marketing and communications department'),
('Sales', 'Sales and business development department'),
('HR', 'Human resources and recruitment department');

-- Insert default admin user
INSERT INTO users (first_name, last_name, email, password_hash, role, department_id) VALUES
('Admin', 'User', 'admin@moderntech.com', 'hashed_password', 'admin', 6);