# WAMP-DS Development History — 17 August 2026

**Milestone:** Continuing real-world development using WAMP-DS  
**Focus:** Developer experience, editor refinement, live preview and usability  
**Project:** WAMP-DS

---

## Another Day Using WAMP-DS

Following yesterday's first real-world development session, development inside WAMP-DS has continued today.

The difference now is that WAMP-DS is no longer being evaluated purely as something being built.

It is being used as the development environment it was intended to become.

That means today's improvements have come naturally from using the software and noticing the things that could make the experience better.

Some have been bugs.

Others have been features that simply needed to exist.

And some have been small usability improvements that only become obvious when you're actually working inside the editor.

---

## Today's Development

The main focus has continued to be the developer experience inside WAMP-DS.

The Code Editor received several usability improvements, building on the Find and Replace functionality added earlier in the session.

`Ctrl+D` was added to duplicate the current line, with selection-aware behaviour allowing only selected content to be duplicated when text is highlighted.

The editor also gained a proper context menu, bringing common operations into the editor through the familiar right-click workflow.

Keyboard support was also refined so that `Ctrl+Shift+S` uses the existing Save As functionality directly.

These are relatively small features individually, but they are exactly the sort of things that make an editor feel more natural to use.

The live preview also received attention after a real-world issue was discovered with CSS changes not always appearing after a normal refresh.

The preview refresh was changed to bypass the browser cache, ensuring that saved changes are reflected immediately in the embedded preview.

This was another example of an issue that only became obvious through actually using WAMP-DS for development.

---

## Development Entries

The individual changes discovered and implemented during today's session are being documented separately.

- **[Development Entry #04 — Find and Replace Added to the Code Editor](./04.md)**
- **[Development Entry #05 — Ctrl+D Line and Selection Duplication](./05.md)**
- **[Development Entry #06 — Preview Cache Refresh](./06.md)**
- **[Development Entry #07 — Editor Context Menu](./07.md)**

More entries will be added as development continues.

---

## The Difference Small Features Make

One thing becoming increasingly apparent during this stage of WAMP-DS is that developer experience is often shaped by very small interactions.

Being able to search without reaching for the mouse.

Being able to replace text without opening another window.

Being able to duplicate a line with a single shortcut.

Being able to duplicate only the code that is currently selected.

Being able to right-click inside the editor and access familiar editing operations.

Being able to save and immediately see CSS changes reflected in the preview.

None of these are particularly large features.

But they remove small interruptions from the development workflow.

And when those interruptions are removed repeatedly throughout a working day, the difference becomes noticeable.

---

## Looking Beyond The Current Build

Today's development also led to the creation of a new `Future` section within the project documentation.

The purpose is to record ideas that may eventually become part of WAMP-DS without presenting them as committed features or promises.

This provides somewhere to capture ideas as they occur during development while keeping them separate from the completed development history.

Early ideas include potential FTP deployment support and future AI-assisted development integration.

The intention is to let these ideas develop naturally rather than forcing them into the current development roadmap.

---

## A Living Development Record

Today's history will continue to grow as development continues.

The intention is not to turn these pages into a traditional changelog listing only completed features.

They are becoming a record of what it is actually like to develop software inside WAMP-DS while WAMP-DS itself is still being developed.

Yesterday was about discovering that distinction.

Today is beginning to show what can come from it.

The software is being used.

Small problems are being noticed.

Features are being added because they are useful.

And future ideas are being recorded as they occur.

**Build it.  
Use it.  
Notice what gets in the way.  
Improve it.  
Keep going.**

---

**[← Back to Development History](../../../)**