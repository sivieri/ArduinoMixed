<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
    version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xsd="http://www.w3.org/2001/XMLSchema"
    xmlns:xmi="http://www.omg.org/XMI"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <xsl:output method="html" encoding="UTF-8"/>
    <xsl:template match="/">
        <html>
            <head>
                <title>Skype History</title>
            </head>
            <body>
                <h1>Skype History</h1>
                <xsl:for-each-group select="//item[action_type/text()='Chat Message']" group-by="chatid">
                    <xsl:sort select="concat(substring(current-group()[1]/action_time, 7, 4), '/', substring(current-group()[1]/action_time, 4, 2), '/', substring(current-group()[1]/action_time, 1, 2))" />
                    <h2><xsl:value-of select="substring(current-group()[1]/action_time, 1, 10)" /></h2>
                    <p>
                        <xsl:for-each select="current-group()">
                            <xsl:sort select="record_number" />
                            <strong><xsl:value-of select="user_name" /></strong>:&#160;<xsl:apply-templates select="chat_message"/><br />
                        </xsl:for-each>
                    </p>
                </xsl:for-each-group>
            </body>
        </html>
    </xsl:template>
    <xsl:template match="chat_message">
        <xsl:apply-templates />
    </xsl:template>
    <xsl:template match="ss">
        <xsl:value-of select="./text()" />
    </xsl:template>
    <xsl:template match="text()">
        <xsl:value-of select="." />
    </xsl:template>
</xsl:stylesheet>
