-- ============================================================
--  Library Management System – Full Database Setup
--  Run this in MySQL Workbench / phpMyAdmin / CLI
-- ============================================================

CREATE DATABASE IF NOT EXISTS library_db;
USE library_db;

-- ── TABLES ───────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS users (
    user_id    INT          AUTO_INCREMENT PRIMARY KEY,
    full_name  VARCHAR(100) NOT NULL,
    username   VARCHAR(50)  NOT NULL UNIQUE,
    email      VARCHAR(100) NOT NULL UNIQUE,
    password   VARCHAR(255) NOT NULL COMMENT 'MD5 hash',
    role       ENUM('Admin','Librarian','Member') DEFAULT 'Member',
    status     ENUM('Active','Inactive')          DEFAULT 'Active',
    created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS books (
    book_id          INT          AUTO_INCREMENT PRIMARY KEY,
    title            VARCHAR(200) NOT NULL,
    author           VARCHAR(100),
    isbn             VARCHAR(20)  UNIQUE,
    available_copies INT          DEFAULT 0,
    total_copies     INT          DEFAULT 0
);

CREATE TABLE IF NOT EXISTS borrowings (
    borrowing_id INT           AUTO_INCREMENT PRIMARY KEY,
    book_id      INT           NOT NULL,
    user_id      INT           NOT NULL,
    borrow_date  DATE          DEFAULT (CURRENT_DATE),
    due_date     DATE,
    return_date  DATE          DEFAULT NULL,
    fine_amount  DECIMAL(8,2)  DEFAULT 0.00,
    FOREIGN KEY (book_id) REFERENCES books(book_id),
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- ── TRIGGERS ─────────────────────────────────────────────────

DELIMITER $$

DROP TRIGGER IF EXISTS trg_after_borrow_insert$$
CREATE TRIGGER trg_after_borrow_insert
AFTER INSERT ON borrowings
FOR EACH ROW
BEGIN
    UPDATE books SET available_copies = available_copies - 1
    WHERE book_id = NEW.book_id;
END$$

DROP TRIGGER IF EXISTS trg_after_borrow_update$$
CREATE TRIGGER trg_after_borrow_update
AFTER UPDATE ON borrowings
FOR EACH ROW
BEGIN
    IF OLD.return_date IS NULL AND NEW.return_date IS NOT NULL THEN
        UPDATE books SET available_copies = available_copies + 1
        WHERE book_id = NEW.book_id;

        IF NEW.return_date > NEW.due_date THEN
            UPDATE borrowings
            SET fine_amount = DATEDIFF(NEW.return_date, NEW.due_date) * 5.00
            WHERE borrowing_id = NEW.borrowing_id;
        END IF;
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_before_borrow_delete$$
CREATE TRIGGER trg_before_borrow_delete
BEFORE DELETE ON borrowings
FOR EACH ROW
BEGIN
    IF OLD.return_date IS NULL THEN
        UPDATE books SET available_copies = available_copies + 1
        WHERE book_id = OLD.book_id;
    END IF;
END$$

DELIMITER ;

-- ── SEED DATA ─────────────────────────────────────────────────

-- Users (password = MD5 of username + '123', e.g. admin123)
INSERT IGNORE INTO users (full_name, username, email, password, role, status) VALUES
  ('System Administrator', 'admin',    'admin@library.com',    MD5('admin123'),    'Admin',     'Active'),
  ('Maria Santos',         'librarian','librarian@library.com', MD5('librarian123'),'Librarian', 'Active'),
  ('Juan dela Cruz',       'juan',     'juan@mail.com',         MD5('juan123'),     'Member',    'Active'),
  ('Ana Reyes',            'ana',      'ana@mail.com',          MD5('ana123'),      'Member',    'Active'),
  ('Carlo Mendoza',        'carlo',    'carlo@mail.com',        MD5('carlo123'),    'Member',    'Active'),
  ('Lena Bautista',        'lena',     'lena@mail.com',         MD5('lena123'),     'Member',    'Active');

-- Books (Christian / Faith-based titles)
INSERT IGNORE INTO books (title, author, isbn, total_copies, available_copies) VALUES
  ('The Purpose Driven Life',       'Rick Warren',     '978-0-31-033750-9', 6, 6),
  ('Mere Christianity',             'C.S. Lewis',      '978-0-06-065292-0', 5, 5),
  ('The Case for Christ',           'Lee Strobel',     '978-0-31-033570-3', 4, 4),
  ('Experiencing God',              'Henry Blackaby',  '978-0-80-549954-1', 3, 3),
  ('The Ragamuffin Gospel',         'Brennan Manning', '978-1-59-052241-2', 4, 4),
  ('Crazy Love',                    'Francis Chan',    '978-1-43-471828-7', 5, 5),
  ('The Screwtape Letters',         'C.S. Lewis',      '978-0-06-065293-7', 3, 3),
  ('Knowing God',                   'J.I. Packer',     '978-0-83-081650-4', 4, 4),
  ('Jesus Calling',                 'Sarah Young',     '978-1-59-145188-9', 6, 6),
  ('The Pursuit of God',            'A.W. Tozer',      '978-1-60-006003-1', 5, 5),
  ('Disciplines of a Godly Man',    'R. Kent Hughes',  '978-1-43-351398-8', 3, 3),
  ('Desiring God',                  'John Piper',      '978-1-59-638824-7', 4, 4);

-- Sample borrowings (using subqueries so IDs stay flexible)
INSERT INTO borrowings (book_id, user_id, borrow_date, due_date) VALUES
  ((SELECT book_id FROM books WHERE isbn='978-0-31-033750-9'),
   (SELECT user_id FROM users WHERE username='juan'),
   CURDATE() - INTERVAL 10 DAY, CURDATE() + INTERVAL 4 DAY),

  ((SELECT book_id FROM books WHERE isbn='978-0-06-065292-0'),
   (SELECT user_id FROM users WHERE username='ana'),
   CURDATE() - INTERVAL 20 DAY, CURDATE() - INTERVAL 6 DAY);  -- overdue → fine on return

-- ── QUICK VERIFICATION ────────────────────────────────────────
SELECT 'Users:'      AS entity, COUNT(*) AS total FROM users
UNION ALL
SELECT 'Books',      COUNT(*) FROM books
UNION ALL
SELECT 'Borrowings', COUNT(*) FROM borrowings;