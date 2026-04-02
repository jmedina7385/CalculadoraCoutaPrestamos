import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const clientApp = path.resolve(scriptDir, "..");
const src = path.join(clientApp, "node_modules", "imask", "dist", "imask.min.js");
const destDir = path.join(clientApp, "..", "wwwroot", "js", "vendor");
const dest = path.join(destDir, "imask.min.js");

if (!fs.existsSync(src)) {
  process.exit(0);
}

fs.mkdirSync(destDir, { recursive: true });
fs.copyFileSync(src, dest);
