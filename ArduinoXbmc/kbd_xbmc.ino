#include <SPI.h>
#include <Ethernet.h>
#include <WebServer.h>

#define KEY_LEFT_CTRL	0x01
#define KEY_LEFT_SHIFT	0x02
#define KEY_RIGHT_CTRL	0x10
#define KEY_RIGHT_SHIFT	0x20
#define PREFIX "/"
#define PORT 80
#define MAXLENGTH 32

uint8_t buf[8] = { 0 };
byte mac[] = {0x90, 0xA2, 0xDA, 0x00, 0x34, 0xD2};
WebServer webserver(PREFIX, PORT);
P(htmlPage) = 
    "<html>"
    "<title>Arduino XBMC Keyboard</title>"
    "</head>"
    "<body>"
    "<form action='/' method='post'>"
    "Command: <input type='text' name='cmd' size='30' maxlength='30' />"
    "<input type='submit' value='Submit' />"
    "</form>"
    "</body>"
    "</html>";

void sendCmd(char* str) {
    char *chp = str;
    while (*chp) {
	    
	if ((*chp >= 'a') && (*chp <= 'z')) {
	    buf[2] = *chp - 'a' + 4;
	} else if ((*chp >= 'A') && (*chp <= 'Z')) {
	    buf[0] = KEY_LEFT_SHIFT;	/* Caps */
	    buf[2] = *chp - 'A' + 4;
        } else if ((*chp >= '0') && (*chp <= '9')) {
            buf[2] = *chp - 12;
	} else {
	    switch (*chp) {
                case '!':
                    buf[0] = KEY_LEFT_SHIFT;
                    buf[2] = 30;
                    break;
                case '?':
                    buf[0] = KEY_LEFT_SHIFT;
                    buf[2] = 56;
                    break;
                case '-':
                    buf[2] = 45;
                    break;
                case '.':
                    buf[2] = 55;
                    break;
                case '/':
                    buf[2] = 56;
                    break;
        	case ' ':
        	    buf[2] = 44;
        	    break;
        	default:
                    buf[0] = KEY_LEFT_SHIFT;
        	    buf[2] = 32;
        	    break;
	    }
	}
	Serial.write(buf, 8);	// Send keypress
	buf[0] = 0;
	buf[2] = 0;
	Serial.write(buf, 8);	// Release key
	chp++;
    }
}

void indexCmd(WebServer &server, WebServer::ConnectionType type, char *, bool) {
    bool repeat;
    char name[MAXLENGTH], value[MAXLENGTH];
    
    if (type == WebServer::GET) {
        server.httpSuccess();
        server.printP(htmlPage);
    }
    else if (type == WebServer::POST) {
        do {
            repeat = server.readPOSTparam(name, MAXLENGTH, value, MAXLENGTH);
            if (strcmp(name, "cmd") == 0) {
                sendCmd(value);
                break;
            }
        } while (repeat);
        server.httpSeeOther("/");
    }
    else {
        server.httpSuccess();
    }
}

void setup() 
{
    Serial.begin(9600);
    delay(200);
    if (Ethernet.begin(mac) == 0) {
        // no point in carrying on, so do nothing forevermore:
        for(;;)
          ;
    }
    webserver.setDefaultCommand(&indexCmd);
    webserver.begin();
}

void loop() 
{
    
    char buff[64];
    int len = 64;

    webserver.processConnection(buff, &len);
    Ethernet.maintain();
}

