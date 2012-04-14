Tweetino
========

This is an implementation of the OAuth authentication mechanism for Twitter on Arduino.

The current version contains the main sketch, tweetino.ino, which connects periodically to Internet, synchronizes the clock using NTP, and then performs the request of the last tweet written by the owner of the keys, showing the resulting text on an LCD screen.
Several parameters have to be added to the code (see the TODO sections), and the secret key pair must have been saved in the Arduino EEPROM using the second sketch provided, eepromtweet.ino.
All the code works on Arduino 1.0 beta 1, and especially the Ethernet DHCP part does not work (here) on previous versions of the IDE; the code has been tested on Arduino Uno.

Remarks
-------

Probably the best implementation of OAuth would be as a library instead that intermingled with the whole application code, but the Arduino Uno resources are quite limited and the whole passing of strings between the application and the library itself would not be easy.
I will probably try to create the library anyway in the next few days, but an Arduino Mega would be useful (and preferable) for testing, especially because of the SRAM quantity (increasing from 2K to 8K, better for string manipulation).

I have also included some external libraries:

* Base64, which you can find here: <https://github.com/adamvr/arduino-base64>
* Cryptosuite (sha1 functions), which you can find here: <http://code.google.com/p/cryptosuite/>

I have included them so you can just put them into <arduino-folder>/libraries/ and use the sketches. I may update them later on.

Known bugs
----------

None so far.

Alessandro Sivieri <alessandro.sivieri@gmail.com>
