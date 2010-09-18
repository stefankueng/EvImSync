<?xml version="1.0" encoding="UTF-8" ?>

<!--
This file is part of the CSharpOptParse .NET C# library
 
The library is hosted at http://csharpoptparse.sf.net

Copyright (C) 2005 by Andrew Robinson

This source code is open source, protected under the GNU GPL Version 2, June 1991
Please see http://opensource.org/licenses/gpl-license.php for information and
specifics on this license.
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:ext="extObj">
	<xsl:output method="text" indent="no" encoding="utf-8" />
	
	<xsl:variable name="bull" select="'&#4; '" />
	
	<xsl:param name="newline" />
	<xsl:param name="title" select="'Usage'" />
	<xsl:param name="shortOptPrefix" select="'-'" />
	<xsl:param name="longOptPrefix" select="'--'" />
	<xsl:param name="includeDefaultValues" select="true()" />
	<xsl:param name="cssStyleSheet" />

	<xsl:template match="/">
		<xsl:apply-templates select="usage/section" />
	</xsl:template>
	
	<xsl:template match="section">
		<xsl:if test="position() &gt; 1">
			<xsl:value-of select="$newline" />
		</xsl:if>
		
		<xsl:call-template name="printText">
			<xsl:with-param name="text" select="@name" />
			<xsl:with-param name="indent" select="count(ancestor::section)" />
		</xsl:call-template>

		<xsl:value-of select="$newline" />
		<xsl:apply-templates />
	</xsl:template>
	
	<xsl:template match="para">
	
		<xsl:if test="position() &gt; 1">
			<xsl:value-of select="$newline" />
		</xsl:if>
		
		<xsl:call-template name="printText">
			<xsl:with-param name="text" select="." />
			<xsl:with-param name="indent" select="count(ancestor::section)" />
		</xsl:call-template>

		<xsl:if test="position() != last() and name(following-sibling::node()) != 'para'">
			<xsl:value-of select="$newline" />
		</xsl:if>
	</xsl:template>
	
	<xsl:template match="list">
		<xsl:apply-templates />
	</xsl:template>
	
	<xsl:template match="item">
		<xsl:choose>
			<xsl:when test="../@type='ordered'">
				<xsl:variable name="num">
					<xsl:number level="single" format="1" />
				</xsl:variable>
				<xsl:variable name="numberWidth" select="string-length(string($num))" />
				
				<xsl:call-template name="printText">
					<xsl:with-param name="text"><xsl:value-of select="$num"/>. <xsl:value-of select="." /></xsl:with-param>
					<xsl:with-param name="indent" select="count(ancestor::section) + count(ancestor::list) - 1" />
					<xsl:with-param name="hanging-indent" select="$numberWidth + 2" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="printText">
					<xsl:with-param name="text"><xsl:value-of select="$bull"/> <xsl:value-of select="." /></xsl:with-param>
					<xsl:with-param name="indent" select="count(ancestor::section) + count(ancestor::list) - 1" />
					<xsl:with-param name="hanging-indent" select="2" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>		
	</xsl:template>
		
	<xsl:template match="arguments">
		<xsl:for-each select="argument">
			<xsl:call-template name="printText">
				<xsl:with-param name="text"><xsl:value-of select="@name" /> (<xsl:if
					test="@optional">Optional, </xsl:if>Type:[<xsl:value-of
					select="@type" />])<xsl:value-of select="$newline" />
					<xsl:value-of select="description"/>
				</xsl:with-param>
				<xsl:with-param name="indent" select="count(ancestor::section)" />
				<xsl:with-param name="hanging-indent" select="4" />
			</xsl:call-template>
			<xsl:if test="last() != position()"><xsl:value-of select="$newline" /></xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template match="category">
		<xsl:call-template name="printText">
			<xsl:with-param name="text">Category: <xsl:value-of select="@name" /></xsl:with-param>
			<xsl:with-param name="indent" select="count(ancestor::section)" />
			<xsl:with-param name="hanging-indent" select="10" />
		</xsl:call-template>
		<xsl:apply-templates />
	</xsl:template>
	
	<xsl:template match="option">
		<xsl:variable name="names">
			<xsl:for-each select="names/name">
				<xsl:if test="position() &gt; 1">, </xsl:if>
				<xsl:choose>
					<xsl:when test="@type='short'"><xsl:value-of select="$shortOptPrefix" /></xsl:when>
					<xsl:otherwise><xsl:value-of select="$longOptPrefix" /></xsl:otherwise>
				</xsl:choose><xsl:value-of select="@value" />					
			</xsl:for-each>
		</xsl:variable>
		
		<xsl:call-template name="printText">
			<xsl:with-param name="text" select="$names" />
			<xsl:with-param name="indent" select="count(ancestor::section) + count(ancestor::category)" />
			<xsl:with-param name="hanging-indent" select="4" />
		</xsl:call-template>
		<xsl:call-template name="printText">
			<xsl:with-param name="text">(Type: <xsl:value-of
				select="@type" /><xsl:if test="valueType">, Value Type:[<xsl:value-of 
				select="valueType" />]</xsl:if><xsl:if 
				test="$includeDefaultValues and defaultValue">, Default Value: '<xsl:value-of 
				select="defaultValue" />'</xsl:if>)</xsl:with-param>
			<xsl:with-param name="indent" select="count(ancestor::section) + count(ancestor::category) + 1" />
			<xsl:with-param name="hanging-indent" select="4" />
		</xsl:call-template>
		<xsl:call-template name="printText">
			<xsl:with-param name="text" select="description" />
			<xsl:with-param name="indent" select="count(ancestor::section) + count(ancestor::category) + 1" />
			<xsl:with-param name="hanging-indent" select="4" />
		</xsl:call-template>
		
		<xsl:if test="last() != position()"><xsl:value-of select="$newline" /></xsl:if>
	</xsl:template>
		
	<xsl:template name="printText">
		<xsl:param name="indent" />
		<xsl:param name="text" />
		<xsl:param name="hanging-indent" select="0" />

		<xsl:value-of select="ext:FormatText($text, $indent * 4, $hanging-indent)" />
	</xsl:template>
</xsl:stylesheet>