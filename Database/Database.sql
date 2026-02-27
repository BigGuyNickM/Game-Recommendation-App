CREATE DATABASE GameStore;
USE GameStore;

CREATE TABLE users (
id INT AUTO_INCREMENT PRIMARY KEY,
username VARCHAR(30) NOT NULL UNIQUE,
email VARCHAR(100) NOT NULL UNIQUE,
created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Switched to discrete rating ('disliked', 'liked', 'loved') & rating value for algorithm
CREATE TABLE ratings (
id INT AUTO_INCREMENT PRIMARY KEY,
rating_name VARCHAR(20) NOT NULL UNIQUE,
rating_value INT NOT NULL UNIQUE
);

CREATE TABLE genres (
id INT AUTO_INCREMENT PRIMARY KEY,
genre_name VARCHAR(100) NOT NULL UNIQUE
);

-- Lists every user's preferred genres (similar to pintrest's 'hobbies & interests')
CREATE TABLE users_preferred_genres (
user_id INT NOT NULL,
genre_id INT NOT NULL,
PRIMARY KEY (user_id, genre_id),
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
FOREIGN KEY (genre_id) REFERENCES genres(id) ON DELETE CASCADE
);

-- Holds all data on each game
CREATE TABLE games (
id INT AUTO_INCREMENT PRIMARY KEY,
title VARCHAR(100) NOT NULL,
publisher VARCHAR(100) NOT NULL,
game_description TEXT NULL,
avg_rating DECIMAL(3,2) NULL,
total_ratings INT UNSIGNED NULL,
created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX idx_game_title ON games(title);
CREATE INDEX idx_game_avg_rating ON games(avg_rating);

CREATE TABLE game_genres (
game_id INT NOT NULL,
genre_id INT NOT NULL,
PRIMARY KEY (game_id, genre_id),
FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE,
FOREIGN KEY (genre_id) REFERENCES genres(id) ON DELETE CASCADE
);

-- Junction table to hold all user game data
CREATE TABLE users_games (
user_id INT NOT NULL,
game_id INT NOT NULL,
rating_id INT NULL,
hours_played INT UNSIGNED NOT NULL DEFAULT 0,
created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
PRIMARY KEY (user_id, game_id),
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE,
FOREIGN KEY (rating_id) REFERENCES ratings(id) ON DELETE SET NULL
);

CREATE TABLE users_wishlist (
    user_id INT NOT NULL,
    game_id INT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, game_id),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE
);

CREATE INDEX idx_users_games_rating ON users_games(rating_id);

SELECT * FROM games;

SELECT COUNT(*) FROM games WHERE total_ratings > 0 AND avg_rating > 0;