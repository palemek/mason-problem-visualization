# Visualization of bricklayer problem

## Overview

### Hi!
This is a program i created to find out more about a math problem i discovered a long time ago(in a high school i think).
With a story it goes like this:
>> There is a bricklayer standing on a edge of a wall circle. He face somewhere towards center at specified angle, and he starts to create a straight wall along his view. He do this until he reach the circle wall. After this encounter, he bounces of it with angle of incidence equals angle of reflectives - like sun beam from mirror, and he continues to go up to next encounter like it. Eventually he encounter his own wall and he bounces like explained above.

![exmple](/example.png)

Im looking for a function of limit of his movement depending on angle that bricklayer starts - exactly approximations of that.

## Description
Program is finding *n* solutions between *starting angle* and *end angle* in linear distribution and drawing final points on screen when he hit in a wall in 90° or when last step is shorter then 0,01 unit.

## Usage
Build mason.sln and run. In windows form:
- *Start Calc* - starting calculations
- *points number* - points to calculate
- *alpha to start* - alpha to start calculations at ( 1 = π/2 = 90°,  0 = 0°)
- *frames num* and *save frames* are to be included later on. They will be used for creating frames for animation through all computed angles 

- left click on picture shows path to get clicked result point 

## Known limitations and bugs
- *save frames* does not have any interface for changing rendering properties
- n was never tested for values greater then 999001
- time for computing can be very big if there are angles to compute being really small(computational error can grow really big really fast)
-for now  program is not taking error of itself into account when computing each path
