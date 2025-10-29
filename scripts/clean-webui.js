#!/usr/bin/env node
const fs = require('fs');
const path = require('path');

const webuiPath = path.join(__dirname, '..', 'VocalFold.WebUI');

// Directories to remove
const dirsToRemove = [
  path.join(webuiPath, '.fable'),
  path.join(webuiPath, 'dist'),
  path.join(webuiPath, 'dist-build'),
  path.join(webuiPath, 'node_modules', '.vite')
];

// Remove generated JS files
function removeJsFiles(dir) {
  if (!fs.existsSync(dir)) return;

  const files = fs.readdirSync(dir, { withFileTypes: true });
  for (const file of files) {
    const fullPath = path.join(dir, file.name);
    if (file.isDirectory()) {
      removeJsFiles(fullPath);
    } else if (file.name.endsWith('.js') || file.name.endsWith('.js.map')) {
      try {
        fs.unlinkSync(fullPath);
        console.log(`Removed: ${fullPath}`);
      } catch (err) {
        console.warn(`Could not remove ${fullPath}: ${err.message}`);
      }
    }
  }
}

// Remove directories
for (const dir of dirsToRemove) {
  if (fs.existsSync(dir)) {
    try {
      fs.rmSync(dir, { recursive: true, force: true });
      console.log(`Removed directory: ${dir}`);
    } catch (err) {
      console.warn(`Could not remove ${dir}: ${err.message}`);
    }
  }
}

// Remove generated JS files from src
const srcPath = path.join(webuiPath, 'src');
if (fs.existsSync(srcPath)) {
  removeJsFiles(srcPath);
}

console.log('WebUI cleanup complete!');
