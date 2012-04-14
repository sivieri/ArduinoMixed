/*
 Copyright (c) 2011 Alessandro Sivieri <alessandro.sivieri@gmail.com>
 
 This library is free software; you can redistribute it and/or
 modify it under the terms of the GNU General Public
 License version 3 as published by the Free Software Foundation.
 
 This library is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 Library General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with this library; see the file COPYING.LIB.  If not, write to
 the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 Boston, MA 02110-1301, USA.
 */

#include <EEPROM.h>

const uint8_t shaKey[] = {
    // TODO set your key here, in bytes, as TokenSecret&AccessTokenSecret
};

void setup() {
    int i;
    
    Serial.begin(115200);
    delay(3000);
    for (i = 0; i < sizeof(shaKey); ++i) {
        EEPROM.write(i, shaKey[i]);
        Serial.print("Wrote byte ");
        Serial.println(i, DEC);
        delay(100);
    }
    EEPROM.write(i, 0x00);
}

void loop() {
    int i = 0;
    byte b;
    
    b = EEPROM.read(i++);
    while (b != 0x00) {
        Serial.print(b, HEX);
        b = EEPROM.read(i++);
    }
    Serial.println();
    Serial.println(i);
    delay(60000);
}
