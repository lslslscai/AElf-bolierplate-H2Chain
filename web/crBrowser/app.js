/**
 * @file app.js
 * @author huangzongzhe
 */

const express = require('express');
const app = express();

const port = process.env.port || '8001';

app.get('/', (req, res) => {
    res.sendfile('index.html');
});

app.get('/index.js', (req, res) => {
    res.sendfile('index.js');
});

app.listen(port, () => {
    console.log('----', port);
});