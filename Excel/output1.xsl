<xsl:stylesheet
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
version="1.0">
<xsl:template match="libraryDatabase">
<HTML>
<BODY>
<p>
<H2>Список книжок</H2> </p>
</BODY>
<BODY>
<TABLE BORDER="2">
<TR>
<TD>
<b>Назва</b>
</TD>
<TD>
<b>Iнформацiя</b>
</TD>
<TD>
<b>Предмет</b>
</TD>
<TD>
<b>Рік видання</b>
</TD>
<TD>
<b>Автори</b>
</TD>
</TR>
<xsl:apply-templates select="book"/>
</TABLE>
</BODY>
</HTML>
</xsl:template>
<xsl:template match="book">
<TR>
<TD>
<b>
<xsl:value-of select="@BK_NAME"/>
</b>
</TD>
<TD><xsl:value-of select="@BK_INFO"/></TD>
<TD><xsl:value-of select="@DC_NAME"/></TD>
<TD><xsl:value-of select="@YEAR"/></TD>
<TD>
<xsl:for-each select="author">
<p>
<xsl:value-of select="@AU_NAME"/>
</p>
</xsl:for-each>
</TD>
</TR>
</xsl:template>
</xsl:stylesheet>
