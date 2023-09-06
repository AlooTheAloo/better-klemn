const http = require("http");
const express = require('express');
const path = require("path");
const fs = require("fs");
const morgan = require("morgan");
const winston = require("winston");

const app = express()
app.use(express.json())
app.use(express.urlencoded())

// Paramètre env
const dotenv = require('dotenv');

// Logger config

// Formatage de winston
const formatConfig = winston.format.combine(
    winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm' }),
    winston.format.printf(
      (info) => `${info.timestamp} ${info.level}: ${info.message}`
    )
);

// Transports pour winston
const transportsConfig = [
    new winston.transports.File({ filename: 'logs/logger.log' }),
    new winston.transports.Console()
];

// Creation de l'objet de log
const logger =  winston.createLogger({
    level: process.env.LOG_LEVEL || 'info',
    format: formatConfig,
    transports: transportsConfig
});

module.exports = logger;

dotenv.config();
const port = process.env.PORT;


app.get('/', (req, res) => {
    res.send("Test");
});

const inscription = require('./inscription')
app.use('/inscription', inscription);


app.listen(port, () => {
    console.log(`[server]: Server is running at http://localhost:${port}`);
});


