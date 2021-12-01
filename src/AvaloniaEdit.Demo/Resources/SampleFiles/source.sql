-- tables
-- Table: call
CREATE TABLE call (
    id int  NOT NULL IDENTITY(1, 1),
    employee_id int  NOT NULL,
    customer_id int  NOT NULL,
    start_time datetime  NOT NULL,
    end_time datetime  NULL,
    call_outcome_id int  NULL,
    CONSTRAINT call_ak_1 UNIQUE (employee_id, start_time),
    CONSTRAINT call_pk PRIMARY KEY  (id)
);
    
-- Table: call_outcome
CREATE TABLE call_outcome (
    id int  NOT NULL IDENTITY(1, 1),
    outcome_text char(128)  NOT NULL,
    CONSTRAINT call_outcome_ak_1 UNIQUE (outcome_text),
    CONSTRAINT call_outcome_pk PRIMARY KEY  (id)
);
    
-- Table: customer
CREATE TABLE customer (
    id int  NOT NULL IDENTITY(1, 1),
    customer_name varchar(255)  NOT NULL,
    city_id int  NOT NULL,
    customer_address varchar(255)  NOT NULL,
    next_call_date date  NULL,
    ts_inserted datetime  NOT NULL,
    CONSTRAINT customer_pk PRIMARY KEY  (id)
);
    
-- Table: employee
CREATE TABLE employee (
    id int  NOT NULL IDENTITY(1, 1),
    first_name varchar(255)  NOT NULL,
    last_name varchar(255)  NOT NULL,
    CONSTRAINT employee_pk PRIMARY KEY  (id)
);
    
-- foreign keys
-- Reference: call_call_outcome (table: call)
ALTER TABLE call ADD CONSTRAINT call_call_outcome
    FOREIGN KEY (call_outcome_id)
    REFERENCES call_outcome (id);
    
-- Reference: call_customer (table: call)
ALTER TABLE call ADD CONSTRAINT call_customer
    FOREIGN KEY (customer_id)
    REFERENCES customer (id);
 
-- Reference: call_employee (table: call)
ALTER TABLE call ADD CONSTRAINT call_employee
    FOREIGN KEY (employee_id)
    REFERENCES employee (id);
 
-- Reference: customer_city (table: customer)
ALTER TABLE customer ADD CONSTRAINT customer_city
    FOREIGN KEY (city_id)
    REFERENCES city (id);
    
-- insert values
INSERT INTO call_outcome (outcome_text) VALUES ('call started');
INSERT INTO call_outcome (outcome_text) VALUES ('finished - successfully');
INSERT INTO call_outcome (outcome_text) VALUES ('finished - unsuccessfully');
    
INSERT INTO employee (first_name, last_name) VALUES ('Thomas (Neo)', 'Anderson');
INSERT INTO employee (first_name, last_name) VALUES ('Agent', 'Smith');
    
INSERT INTO customer (customer_name, city_id, customer_address, next_call_date, ts_inserted) VALUES ('Jewelry Store', 4, 'Long Street 120', '2020/1/21', '2020/1/9 14:1:20');
INSERT INTO customer (customer_name, city_id, customer_address, next_call_date, ts_inserted) VALUES ('Bakery', 1, 'Kurfürstendamm 25', '2020/2/21', '2020/1/9 17:52:15');
INSERT INTO customer (customer_name, city_id, customer_address, next_call_date, ts_inserted) VALUES ('Café', 1, 'Tauentzienstraße 44', '2020/1/21', '2020/1/10 8:2:49');
INSERT INTO customer (customer_name, city_id, customer_address, next_call_date, ts_inserted) VALUES ('Restaurant', 3, 'Ulica lipa 15', '2020/1/21', '2020/1/10 9:20:21');
    
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (1, 4, '2020/1/11 9:0:15', '2020/1/11 9:12:22', 2);
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (1, 2, '2020/1/11 9:14:50', '2020/1/11 9:20:1', 2);
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (2, 3, '2020/1/11 9:2:20', '2020/1/11 9:18:5', 3);
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (1, 1, '2020/1/11 9:24:15', '2020/1/11 9:25:5', 3);
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (1, 3, '2020/1/11 9:26:23', '2020/1/11 9:33:45', 2);
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (1, 2, '2020/1/11 9:40:31', '2020/1/11 9:42:32', 2);
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (2, 4, '2020/1/11 9:41:17', '2020/1/11 9:45:21', 2);
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (1, 1, '2020/1/11 9:42:32', '2020/1/11 9:46:53', 3);
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (2, 1, '2020/1/11 9:46:0', '2020/1/11 9:48:2', 2);
INSERT INTO call (employee_id, customer_id, start_time, end_time, call_outcome_id) VALUES (2, 2, '2020/1/11 9:50:12', '2020/1/11 9:55:35', 2);