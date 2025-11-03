# VocalFold Skills

This directory contains custom skills for the VocalFold project.

## Available Skills

### update-changelog

**Usage**: Invoke this skill when you need to update the changelog

**Description**: This skill helps maintain the changelog by:
1. Updating the CHANGELOG.md file with new entries
2. Automatically regenerating the Changelog.fs component to match

**How to use**:
1. Simply tell Claude what changes you want to add to the changelog
2. The skill will update both files and keep them in sync

**Example prompts**:
- "Add a new feature to the changelog: User profile management"
- "Create a new version 1.1.0 release with the unreleased changes"
- "Add a bug fix to the changelog: Fixed crash on startup"

## Creating New Skills

Skills are markdown files that contain instructions for specialized tasks. To create a new skill:

1. Create a new `.md` file in this directory
2. Write clear instructions for what the skill should do
3. Include examples and guidelines
4. The skill will be automatically available to Claude
