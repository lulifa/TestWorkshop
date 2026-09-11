import fs from "node:fs";
import { createRequire } from "node:module";
import path from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const require = createRequire(path.join(process.cwd(), "package.json"));
const { chromium } = require("playwright");

const docsDir = path.dirname(fileURLToPath(import.meta.url));
const outputDir = path.join(docsDir, "current");
const htmlPath = path.join(docsDir, "current-project.html");
const chromePath =
  process.env.CHROME_PATH ??
  "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";

const diagrams = [
  ["architecture", "01-architecture.png"],
  ["telemetry", "02-telemetry-flow.png"],
  ["deployment", "03-deployment.png"],
  ["versions", "04-versions-and-requirements.png"],
];

fs.mkdirSync(outputDir, { recursive: true });

const browser = await chromium.launch({
  executablePath: chromePath,
  headless: true,
});

try {
  const page = await browser.newPage({
    deviceScaleFactor: 1.5,
    viewport: { height: 1100, width: 1700 },
  });
  await page.goto(pathToFileURL(htmlPath).href, {
    waitUntil: "networkidle",
  });

  for (const [id, filename] of diagrams) {
    await page.locator(`#${id}`).screenshot({
      path: path.join(outputDir, filename),
    });
  }
} finally {
  await browser.close();
}
