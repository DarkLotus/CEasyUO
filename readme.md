 -- Functionality Changes

 - CONTPOSX/Y and CONTSIZEX/Y have been replaced with GUMPPOSX/Y and GUMPSIZEX/Y
   These only update when a new gump opens, and do not update when you change focus in game.
 - #GUMPSERIAL and #GUMPTYPE #LGUMPBUTTON new variables for gumps.


 - Mobile Health bars do not show Type, Use #LTARGETTYPE


--New commands

---Gump Handling

- event gump wait {timeout}
Waits until timeout( default 10 seconds) or a gump appears

- event gump last
Repeats the last gump input action

- event gump button {index}
Responds to the current gump with the specified index


--- Context Menus

- event contextmenu {serial} {index}
Triggers a context menu click at the specified index on the specified object


--Depreceated commands

 - Menu
