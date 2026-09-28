-- Active: 1775099869645@@127.0.0.1@3306@toko_buku
CREATE DATABASE TOKO_BUKU;

create Table user (
    id_user INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) UNIQUE,
    password VARCHAR(255) ,
    role VARCHAR(20) 
);


CREATE TABLE books (
    id_buku INT AUTO_INCREMENT PRIMARY KEY,
    kode_buku VARCHAR(20) UNIQUE,
    judul VARCHAR(150) ,
    pengarang VARCHAR(100) ,
    penerbit VARCHAR(100) ,
    harga DECIMAL(12,2),
    stok INT 
);

CREATE TABLE transactions (
    id_transaksi INT AUTO_INCREMENT PRIMARY KEY,
    tanggal DATETIME ,
    total_harga DECIMAL ,
    id_user INT);




CREATE TABLE transaction_details (
    id_detail INT AUTO_INCREMENT PRIMARY KEY,
    id_transaksi INT ,
    id_buku INT ,
    jumlah INT ,
    subtotal DECIMAL);

INSERT INTO books
(id_buku, kode_buku, judul, pengarang, penerbit, harga, stok)
VALUES
(1, 'BK001', 'Pemrograman Python', 'A. Sudrajat', 'Informatika', 120000, 50),
(2, 'BK002', 'Dasar-Dasar Jaringan', 'B. Wibowo', 'Andi Publisher', 100000, 30),
(3, 'BK003', 'Desain Grafis Modern', 'C. Ramadhan', 'Media Nusantara', 150000, 20);

INSERT INTO transactions
(id_transaksi, tanggal, total_harga, id_user)
VALUES
(1, '2025-01-08 10:30:00', 270000, 2);

INSERT INTO transaction_details
(id_detail, id_transaksi, id_buku, jumlah, subtotal)
VALUES
(1, 1, 1, 1, 120000),
(2, 1, 3, 1, 150000);

insert into user (username, password, role) values 
('admin', 'admin123', 'admin'),
('kasir', 'kasir123', 'kasir');
 SELECT nama FROM kabupaten where id = 11 ;