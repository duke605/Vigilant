# Vigilant
A discord bot that allows any* user to moderate the server. Users can moderate the server by reporting other users and bot will add strikes toward the reported's account. If the reported user accumulates a configurable number of strikes action will be take.

<sub>* Provided the user isn't ignored or has too many strikes*</sub>

# Commands

`!kick <user> - Adds a "kick" strike to the user. User parameter must be a mention (Eg. @User)†`

`!mute <user> - Adds a "mute" strike to the user. User parameter must be a mention (Eg. @User)†`

`!forgive <user> - Removes all permanent strikes from the user. User parameter must be a mention (Eg. @User)*`

```
!inspect <-t table> [-r resolve_ids]*

-t, --table       (R) <enum> The table you want to inspect. (Table names found below. Pluralize them)
-r, --resolve_ids (O) <bool> Wheather or not to resolve ids into names.
```

```
!exempt <-r roles> [-d remove]*

-r, --roles  (R) <List[string]> The roles to add to the exempt table.
-d, --remove (O) <bool> Wheather or not to remove the supplied roles from the exempt table.
```

```
!config [-k kick_num] [-m mute_num] [-f pban_num] [-b block_num] [-M mute_time] [-s allow_mute] [-l allow_kick] [-F allow_pban]

-k, --kick_num   (O) <int> The number of strikes needed to kick a user.
-m, --mute_num   (O) <int> The number of strikes need to mute a user.
-f, --pban_num   (O) <int> The number of permanent strikes a user can recieve before being permanently banned.**
-b, --block_num  (O) <int> The number of concurrent strikes a user can recieve before being ignored by the bot.
-M, --mute_time  (O) <int> The number of minutes a user will be muted for in minutes.
-s, --allow_mute (O) <bool> Wheather or not users can report users to mute them.
-l, --allow_kick (O) <bool> Wheather or not users can report users to kick them.
-F, --allow_pban (O) <bool> Wheather or not users can be permanently banned.
```

<sub>* Server owner only command.</sub><br />
<sub>** Permanent strikes are added when the a user is kicked or muted when they reach max strikes for those types.</sub><br />
<sub>† Supplied user cannot be server owner, exempt, a bot, or command caller.</sub><br />
<sub>R Required parameter.</sub><br />
<sub>O Optional parameter.</sub><br />

# Database
<img src="http://i.imgur.com/h5WIrI1.png" alt="" />

# License
```
The MIT License (MIT)

Copyright (c) 2016-2016 Cole Denslow.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```
