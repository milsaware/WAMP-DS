# WAMP-DS Development History — 16 August 2026

**Milestone:** First real-world development session using WAMP-DS  
**Focus:** Developer experience and real-world usability  
**Project:** WAMP-DS

---

## From Building WAMP-DS to Using WAMP-DS

Although WAMP-DS had been developed and tested extensively, 15 August 2026 marked an important change in perspective.

Until this point, most of the development had naturally been approached from the perspective of the architect and developer. Features were designed, implemented and tested individually, with the goal of making the application work.

It was time to use it for its intended purpose.

WAMP-DS wasn't created simply as an exercise in building a WPF application or as a demonstration of C# development. It was created because there was a genuine need for a development environment that brought together the tools and workflow required for web development.

There is a simple test for whether a developer tool is worth building:

> **If you can build something that you find genuinely useful yourself, then it is worth building for that reason alone.**

So, after spending a considerable amount of time working on WAMP-DS itself, a break from C# was needed.

A couple of simple games were created using the WAMP-DS interface.

What started as a pallet cleanser from C# turned out to be the first proper opportunity to experience WAMP-DS as a developer rather than as its architect.

---

## Wearing a Different Hat

Using WAMP-DS for actual development exposed a different class of problems.

When building the software, the natural questions tend to be architectural:

- Does the project manager work?
- Does the editor open files?
- Does the preview work?
- Does the server start?
- Are the various managers doing what they are supposed to do?

When actually developing inside the application, the questions become much more practical:

- Does the workflow make sense?
- Are files and folders created where I expect them?
- Can I get on with development without unnecessary steps?
- Does the application behave naturally when I use it?

The distinction is important because a feature can be technically correct while still producing a frustrating developer experience.

The games exposed several of these issues.

---

## The First Real-World Findings

The issues discovered during this session were not the result of deliberately testing individual features.

They appeared naturally while trying to make something.

That made them particularly valuable.

Instead of asking whether a particular component worked in isolation, WAMP-DS was now being tested as a complete development environment.

The problems discovered during this session would become the next targets for improvement.

---

## A Useful Change in Perspective

This session established something important about the future development of WAMP-DS.

The software now has to satisfy two perspectives simultaneously.

The **architect** needs the underlying design to be sound.

The **developer** needs the resulting workflow to be useful.

Neither perspective can replace the other.

The architecture determines how WAMP-DS works.

Using WAMP-DS determines whether that architecture actually produces a good development experience.

That means real development is now part of the development process itself.

And that is exactly how it should be.

---

## What Comes Next

The issues discovered while creating the games will now be addressed one by one.

Each fix will be recorded as part of this day's development history, including what was discovered, why it mattered, how it was addressed and how the result was verified.

The project has moved from:

**Building the development environment**

to:

**Building the development environment while actually using it.**

That's a much better test.

---

**[← Back to Development History](../../../)**