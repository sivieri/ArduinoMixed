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
#include <SPI.h>
#include <Ethernet.h>
#include <Udp.h>
#include <LiquidCrystal.h>
#include <sha1.h>
#include <Base64.h>
#include <string.h>

#define NTP_PACKET_SIZE 48
#define EPOCH 2208988800UL
#define LOCAL_PORT 8888
#define MAXTRIALS  3
#define LCDROWS 16
#define LCDCOLS 2

// Ethernet
byte mac[] = {0x00, 0x11, 0x22, 0x33, 0x44, 0x55}; // TODO set your value here
Client client;
// NTP, the time server can be changed
IPAddress timeServer(193, 204, 114, 232);
byte packetBuffer[NTP_PACKET_SIZE];
UDP Udp;
// LCD
LiquidCrystal lcd(9, 8, 7, 6, 5, 2);
// OAuth parameters
uint8_t shaKey[100];
int shaKeyLength;
const char consumerKey[] = "oauth_consumer_key";
const char consumerValue[] = ""; // TODO set your value here
const char tokenKey[] = "oauth_token";
const char tokenValue[] = ""; // TODO set your value here
const char signatureMethodKey[] = "oauth_signature_method";
const char signatureMethodValue[] = "HMAC-SHA1";
const char versionKey[] = "oauth_version";
const char versionValue[] = "1.0";
const char signatureKey[] = "oauth_signature";
const char nonceKey[] = "oauth_nonce";
const char timestampKey[] = "oauth_timestamp";
const char proto[] = "http://";
const char protoEncoded[] = "http%3A%2F%2F";
const char serverName[] = "api.twitter.com";
const char method[] = "GET";
const char path[] = "/1/statuses/user_timeline.xml?";
const char pathEncoded[] = "%2F1%2Fstatuses%2Fuser_timeline.xml&";
const char params[] = "count=1&include_rts=1";
const char paramsEncoded[] = "count%3D1%26include_rts%3D1";
// Tweets
int trials = 0, tweeted = 0, led = 1;

// See the NTP example sketch for details
unsigned long sendNTPpacket()
{
    memset(packetBuffer, 0, NTP_PACKET_SIZE); 
    packetBuffer[0] = 0b11100011;
    packetBuffer[1] = 0;
    packetBuffer[2] = 6;
    packetBuffer[3] = 0xEC;
    packetBuffer[12]  = 49; 
    packetBuffer[13]  = 0x4E;
    packetBuffer[14]  = 49;
    packetBuffer[15]  = 52;
    Udp.beginPacket(timeServer, 123);
    Udp.write(packetBuffer, NTP_PACKET_SIZE);
    Udp.endPacket(); 
}

void printByteArray(uint8_t* ar, int length) {
    int i;
    
    for (i =  0; i < length; i++) {
        // Serial.print("0123456789abcdef"[ar[i]>>4]);
        // Serial.print("0123456789abcdef"[ar[i]&0xf]);
    }
    // Serial.println();
}

void binaryToString(uint8_t* hash, char* buf, int length) {
    int i;
    
    for (i =  0; i < length; i++) {
        buf[i] = (char) hash[i];
    }
}

void urlencode(char* dest, char* src) {
    int i, counter = 0;
    char c;
    char buf[3];
    
    for (i = 0; i < strlen(src); i++) {
        c = src[i];
        if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_' || c == '.' || c == '~') {
            dest[counter++] = c;
        }
        else {
            dest[counter++] = '%';
            snprintf(buf,3 * sizeof(char), "%02x", c & 0xff);
            dest[counter++] = buf[0];
            dest[counter++] = buf[1];
        }
    }
    dest[counter] = '\0';
}

void printTweet(char* text) {
    int i, row = 0;
    
    // Serial.print("Tweet: ");
    // Serial.println(text);
    lcd.clear();
    for (i = 0; i < LCDROWS * 2 - 2; ++i) {
        if (text[i] == '\0') {
            break;
        }
        if (i % LCDROWS == 0) {
            lcd.setCursor(0, row++);
        }
        lcd.print(text[i]);
    }
    if (text[i] != '\0') {
        lcd.print("..");
    }
    tweeted = 1;
}

void setup() {
    int i;
    byte b;
    
    // Setup
    // Serial.begin(115200);
    pinMode(3, OUTPUT);
    analogWrite(3, 40);
    pinMode(10, OUTPUT);
    digitalWrite(10, HIGH);
    pinMode(13, OUTPUT);
    pinMode(led, OUTPUT);
    digitalWrite(led, LOW);
    lcd.begin(LCDROWS, LCDCOLS);
    lcd.clear();
    delay(2000);
    
    // Ethernet
    // Serial.println("Initializing Ethernet stack...");    
    while (Ethernet.begin(mac) == 0) {
        // Serial.println("DHCP failed");
        delay(5000);
    }
    Udp.begin(LOCAL_PORT);
    // Serial.print("My IP address: ");
    for (byte thisByte = 0; thisByte < 4; thisByte++) {
        // Serial.print(Ethernet.localIP()[thisByte], DEC);
        // Serial.print("."); 
    }
    // Serial.println();
    lcd.print("My IP address:");
    lcd.setCursor(0, 1);
    for (byte thisByte = 0; thisByte < 4; thisByte++) {
        lcd.print(Ethernet.localIP()[thisByte], DEC);
        lcd.print("."); 
    }
    
    // EEPROM SHA-HMAC key
    // TODO save your value in EEPROM, for example using the other sketch
    i = 0;
    b = EEPROM.read(i);
    while (b != 0x00) {
        shaKey[i++] = b;
        b = EEPROM.read(i);
    }
    shaKeyLength = i;
    // Serial.print("SHA-HMAC key: ");
    printByteArray(shaKey, shaKeyLength);
    
    // Serial.println("---");
}

void loop() {
    unsigned long highWord, lowWord, currentTime, timestampValue;
    char buf[LCDROWS * 2 + 1], buf2[30], signatureValue[50];
    long nonceValue;
    uint8_t* dest;
    char c;
    char* pc;
    int counter, code = -1, httpStatus = 0, httpContinue = 1, tweet = 0;
    
    // Init
    tweeted = 0;
    trials = 0;
    digitalWrite(led, HIGH);
    // First of all: timestamp
    // Serial.println("Synchronizing with NTP...");
    sendNTPpacket();
    delay(2000);
    if (Udp.parsePacket()) {
        Udp.read(packetBuffer,NTP_PACKET_SIZE);
        highWord = word(packetBuffer[40], packetBuffer[41]);
        lowWord = word(packetBuffer[42], packetBuffer[43]);
        // this is NTP time (seconds since Jan 1 1900):
        currentTime = highWord << 16 | lowWord;
        // this is Unix time (seconds since Jan 1 1970):
        timestampValue = currentTime - EPOCH;
        // Serial.print("Timestamp: ");
        // Serial.println(timestampValue);
        
        // Next: nonce
        randomSeed(analogRead(0));
        while (tweeted == 0 && trials < MAXTRIALS) {
            nonceValue = random(10000);
            
            // Next: base string and signature
            Sha1.initHmac(shaKey, shaKeyLength);
            Sha1.print(method);
            Sha1.print('&');
            Sha1.print(protoEncoded);
            Sha1.print(serverName);
            Sha1.print(pathEncoded);
            Sha1.print(paramsEncoded);
            Sha1.print("%26");
            Sha1.print(consumerKey);
            Sha1.print("%3D");
            Sha1.print(consumerValue);
            Sha1.print("%26");
            Sha1.print(nonceKey);
            Sha1.print("%3D");
            Sha1.print(nonceValue);
            Sha1.print("%26");
            Sha1.print(signatureMethodKey);
            Sha1.print("%3D");
            Sha1.print(signatureMethodValue);
            Sha1.print("%26");
            Sha1.print(timestampKey);
            Sha1.print("%3D");
            Sha1.print(timestampValue);
            Sha1.print("%26");
            Sha1.print(tokenKey);
            Sha1.print("%3D");
            Sha1.print(tokenValue);
            Sha1.print("%26");
            Sha1.print(versionKey);
            Sha1.print("%3D");
            Sha1.print(versionValue);
            dest = Sha1.resultHmac();
            // Serial.print("Signature: ");
            binaryToString(dest, buf, 20);
            base64_encode(buf2, buf, 20);
            urlencode(signatureValue, buf2);
            // Serial.println(signatureValue);
            // Serial.println("---");
            
            // Next: HTTP request
            // Serial.print("Connecting...");
            if (client.connect(serverName, 80)) {
                // Serial.println(" connected!");
                client.print(method);
                client.print(' ');
                client.print(path);
                client.print(signatureKey);
                client.print('=');
                client.print(signatureValue);
                client.print('&');
                client.print(versionKey);
                client.print('=');
                client.print(versionValue);
                client.print('&');
                client.print(nonceKey);
                client.print('=');
                client.print(nonceValue);
                client.print('&');
                client.print(timestampKey);
                client.print('=');
                client.print(timestampValue);
                client.print('&');
                client.print(signatureMethodKey);
                client.print('=');
                client.print(signatureMethodValue);
                client.print('&');
                client.print(consumerKey);
                client.print('=');
                client.print(consumerValue);
                client.print('&');
                client.print(tokenKey);
                client.print('=');
                client.print(tokenValue);
                client.print('&');
                client.print(params);
                client.println(" HTTP/1.1");
                client.println("Host: api.twitter.com");
                client.println("Connection: keep-alive");
                client.println();
                
                // Next: get response
                delay(1000);
                counter = 0;
                for (;;) {
                    if (client.available()) {
                        c = client.read();
                        if (c == '\n') {
                            if (code == 200 && tweet == 1) {
                                buf[counter ] = '\0';
                                pc = strrchr(buf, '<');
                                if (pc != NULL) {
                                    *pc = '\0';
                                }
                                printTweet(buf);
                                tweet = 0;
                            }
                            counter = 0;
                            httpContinue = 1;
                        }
                        else if (httpContinue == 1) {
                            // Full buffer, no need to do more here (LCD is limited to 32 characters, so...)
                            if (counter == LCDROWS * 2) {
                                httpContinue = 0;
                                continue;
                            }
                            buf[counter++] = c;
                            if (httpStatus == 0 && !tweet && counter == 8) {
                                buf[counter] = '\0';
                                if (strcmp(buf, "Status: ") != 0 ) {
                                    httpContinue = 0;
                                }
                                else {
                                    httpStatus = 1;
                                    counter = 0;
                                }
                            }
                            if (httpStatus == 1 && counter == 3) {
                                buf[counter] = '\0';
                                code = atoi(buf);
                                // Serial.print("Code: ");
                                // Serial.println(code);
                                httpContinue = 0;
                                httpStatus = 0;
                            }
                            if (code == 200 && tweet == 0 && counter == 8) {
                                buf[counter] = '\0';
                                if (strcmp(buf, "  <text>") != 0) {
                                    httpContinue = 0;
                                }
                                else {
                                    tweet = 1;
                                    counter = 0;
                                    httpContinue = 1;
                                }
                            }
                        }
                    }
                    if (!client.connected()) {
                        client.stop();
                        break;
                    }
                }
            }
            else {
                // Serial.println(" not connected");
            }
            if (tweeted == 0) {
                ++trials;
                // Serial.println("Retrying (if possible)...");
            }
        }
    }
    else {
        // Serial.println("NTP did not respond in time.");
    }
    
    // Waiting for the next trip (10 minutes)
    // Serial.println("Sleeping...");
    digitalWrite(led, LOW);
    delay(600000);
}

