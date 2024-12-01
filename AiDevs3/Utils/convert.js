import TurndownService from 'turndown';

const base64Html = process.argv[2];
const html = Buffer.from(base64Html, 'base64').toString('utf-8');
const turndownService = new TurndownService();
console.log(turndownService.turndown(html));
