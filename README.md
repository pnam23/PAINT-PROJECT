# Project PAINT

## Team members:
- 21127080 - Nguyễn Đắc Khôi
- 21127100 - Phạm Phúc Lộc
- 21127157 - Dương Phước Sang
- 21127647 - Lê Nguyễn Phương Nam

## Technical details
- Design Pattern: singleton, prototype

## Core requirements
1. Dynamically load all graphic objects that can be drawn from external DLL files.
2. The user can choose which object to draw.
3. The user can see the preview of the object they want to draw.
4. The user can finish the drawing preview and their change becomes permanent with previously drawn objects.
5. The list of drawn objects can be saved and loaded again for continuing later.
6. Save and load all drawn objects as an image in bmp/png/jpeg format (rasterization).

## Basic graphic objects
1. Line: controlled by two points, the starting point, and the endpoint.
2. Rectangle: controlled by two points, the left top point, and the right bottom point.
3. Ellipse: controlled by two points, the left top point, and the right bottom point.

## Improvements
1. Allow the user to change the color, pen width, stroke type (dash, dot, dash dot dot...)
2. Adding image to the canvas
3. Select a single element for editing again
   Resize
   Move
4. Cut / Copy / Paste
5. Undo, Redo (Command)
6. Use Visitor design pattern (Suggestion: save objects into text file / xml file / json file / binary file)
7. Used Fluent.Ribbon to obtain MS Paint-like user interface.
8. Objects in drawable list have their own icons instead of plain text.
9. To begin working again right away, click the "New File" button. If any new changes are found, the program will naturally prompt the user for confirmation.
10. Allow user to draw freestyle and erase by mouse
11. Package wpf application

## Weakness:
This project still has some mistakes and needs to be improved. We can select and do actions (copy, paste, cut) on the stroke but not on the shape.

## Self-rating:
- 21127080 - Nguyễn Đắc Khôi: 10
- 21127100 - Phạm Phúc Lộc: 10
- 21127157 - Dương Phước Sang: 10 
- 21127647 - Lê Nguyễn Phương Nam: 10

## Link demo:
https://youtu.be/RBKoqpZkV5A
