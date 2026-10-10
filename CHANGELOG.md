# Changelog

All notable changes to this project are documented here. The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

## [0.1.0] - 2026-10-10

### Added
- Overflow handling: third-party toolbar buttons that don't fit are scrolled; vanilla buttons stay put.
- ▲▼ buttons in the top row of the scrolling area (hold to repeat), a divider line between fixed and scrolling rows, and tooltips with the number of rows left in each direction.
- Rows hidden behind the studio's main menu no longer count as usable space.
- Language setting (Auto, English, 简体中文, 日本語) and optional, unbound-by-default scroll hotkeys.
- Debug option `Fake buttons` to test overflow.
- Buttons dragged out of the two toolbar columns stay where they are.
- Requires KKAPI 1.45 or later; tested with 1.45.1 and 1.48.1.
