<?xml version="1.0" encoding="UTF-8" ?>

<!--
This file is part of the CSharpOptParse .NET C# library
 
The library is hosted at http://csharpoptparse.sf.net

Copyright (C) 2005 by Andrew Robinson

This source code is open source, protected under the GNU GPL Version 2, June 1991
Please see http://opensource.org/licenses/gpl-license.php for information and
specifics on this license.
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" indent="yes" encoding="utf-8" />
	
	<xsl:param name="title" select="'Usage'"/>
	<xsl:param name="shortOptPrefix" select="'-'" />
	<xsl:param name="longOptPrefix" select="'--'"/>
	<xsl:param name="includeDefaultValues" select="true()" />
	<xsl:param name="cssStyleSheet" />
	
	<xsl:variable name="space">&#160;</xsl:variable>

	<xsl:template match="/">
		<html>
			<head>
				<xsl:if test="$title"><title><xsl:value-of select="$title" /></title></xsl:if>
				<xsl:choose>
					<xsl:when test="$cssStyleSheet">
						<link rel="stylesheet" type="text/css" href="[$cssStyleSheet]" />
					</xsl:when>
					<xsl:otherwise>
						<style type="text/css">
<xsl:text disable-output-escaping="yes">&lt;--</xsl:text>
body { font: bolder medium serif; }

.section { font: bolder larger Helvetica, sans-serif; }
h1.section { font: bolder x-large; }
h2.section { font: bolder x-large; }
h3.section { font: bolder large; }

table.arguments 
{
	border: 1px solid black;
	border-collapse: collapse;
}
tr.arguments {}
th.arguments 
{
	background-color: CCCCCC;
	border: 1px solid #555555;
	padding: 2px 6px; 
	border-spacing: 5;
	text-align: left;
}

tr.argument  {}
td.argument 
{ 
	border: 1px solid #AAAAAA;
	padding: 2px 6px; 
	border-spacing: 5;
}

tr.category {}
th.category 
{
	border: 1px solid #555555;
	border-collapse: collapse;
	border-spacing: 5;
	padding: 2px 6px; 
	background-color: CCCCCC;
	text-align: left;
}
td.category 
{
	border: 1px solid #AAAAAA;
	border-collapse: collapse;
	border-spacing: 5;
	background-color: DDDDDD;
	font-weight: bold;
	padding: 2px 6px; 
}
td.catcell 
{
	border: 0px solid black;
	border-collapse: collapse;
	width: 25px;
}

table.options 
{
	border: 1px solid black;
	border-collapse: collapse;
	border-spacing: 5;
}
tr.options {}
th.options 
{
	background-color: CCCCCC;
	border: 1px solid #555555;
	padding: 2px 6px; 
	border-spacing: 5;
	text-align: left;
}

tr.option {}
td.option
{ 
	border: 1px solid #AAAAAA;
	padding: 2px 6px; 
	border-spacing: 5;
}
td.optdefval
{
	text-align: center;
}
<xsl:text disable-output-escaping="yes">--&gt;</xsl:text>
						</style>
					</xsl:otherwise>
				</xsl:choose>				
			</head>
			<body>
				<xsl:apply-templates select="usage/section" />
			</body>
		</html>
	</xsl:template>
	
	<xsl:template match="section">
		<xsl:variable name="number" select="count(ancestor-or-self::section)" />
		
		<xsl:text disable-output-escaping="yes">&lt;h</xsl:text>
		<xsl:value-of select="$number" /> class="section"
		<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:value-of select="@name" />
		<xsl:text disable-output-escaping="yes">&lt;/h</xsl:text>
		<xsl:value-of select="$number" />
		<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:apply-templates />
		
	</xsl:template>
	
	<xsl:template match="para">
		<p><xsl:apply-templates /></p>
	</xsl:template>
	
	<xsl:template match="item">
		<li class="listItem"><xsl:value-of select="." /></li>
	</xsl:template>
	
	<xsl:template match="list">
		<xsl:choose>
			<xsl:when test="@type='ordered'">
				<ol class="list">
					<xsl:apply-templates />
				</ol>
			</xsl:when>
			<xsl:otherwise>
				<ul class="list">
					<xsl:apply-templates />
				</ul>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
		
	<xsl:template match="arguments">
		<table class="arguments" border="0">
			<tr class="arguments">
				<th class="arguments">Name</th>
				<th class="arguments">Optional</th>
				<th class="arguments">Expected Type</th>
				<th class="arguments">Description</th>
			</tr>
			<xsl:for-each select="argument">
				<tr class="argument">
					<td valign="top" class="argument argname"><xsl:value-of select="@name" /></td>
					<td valign="top" class="argument argopt"><xsl:value-of select="@optional" /></td>
					<td valign="top" class="argument argtype"><xsl:value-of select="@type" /></td>
					<td valign="top" class="argument argdesc"><xsl:apply-templates select="description"/></td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>
	
	<xsl:template match="options">
		<table class="options" border="0">
			<xsl:if test="category">
				<xsl:variable name="colspan">
					<xsl:choose>
						<xsl:when test="$includeDefaultValues">6</xsl:when>
						<xsl:otherwise>5</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<tr class="category">
					<th class="category" colspan="{$colspan}">Category</th>
				</tr>
			</xsl:if>
			<tr class="options">
				<xsl:if test="category"><th><xsl:value-of select="$space" /></th></xsl:if>
				<th class="options">Names</th>
				<th class="options">Type</th>
				<th class="options">Expected Value Type</th>
				<th class="options">Description</th>
				<xsl:if test="$includeDefaultValues">
					<th class="options">Default Value</th>
				</xsl:if>
			</tr>
			<xsl:apply-templates />
		</table>
	</xsl:template>
	
	<xsl:template match="category">
		<xsl:variable name="colspan">
			<xsl:choose>
				<xsl:when test="$includeDefaultValues">6</xsl:when>
				<xsl:otherwise>5</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<tr>
			<td colspan="{$colspan}" class="category"><xsl:value-of select="@name" /></td>
		</tr>
		<xsl:apply-templates />
	</xsl:template>
	
	<xsl:template match="option">
		<tr>
			<xsl:if test="name(..) = 'category'"><td class="catcell"><xsl:value-of 
				select="$space" /></td></xsl:if>
			<td valign="top" class="option optname">
				<xsl:for-each select="names/name">
					<xsl:if test="position() &gt; 1">, </xsl:if>
					<xsl:choose>
						<xsl:when test="@type='short'"><xsl:value-of select="$shortOptPrefix" /></xsl:when>
						<xsl:otherwise><xsl:value-of select="$longOptPrefix" /></xsl:otherwise>
					</xsl:choose><xsl:value-of select="@value" />
				</xsl:for-each>
			</td>
			<td valign="top" class="option opttype"><xsl:value-of select="@type"/></td>
			<td valign="top" class="option optvaltype">
				<xsl:choose>
					<xsl:when test="valueType"><xsl:value-of select="valueType" /></xsl:when>
					<xsl:otherwise><xsl:value-of select="$space" /></xsl:otherwise>
				</xsl:choose>
			</td>
			<td valign="top" class="option optdesc"><xsl:apply-templates select="description" /></td>
			<td valign="top" class="option optdefval">
				<xsl:choose>
					<xsl:when test="$includeDefaultValues and defaultValue">
						<xsl:value-of select="defaultValue" />
					</xsl:when>
					<xsl:otherwise><xsl:value-of select="$space" /></xsl:otherwise>
				</xsl:choose>
			</td>
		</tr>
	</xsl:template>
	
	<xsl:template match="description">
		<xsl:apply-templates select="node()|@*|*" />
	</xsl:template>
	
	<xsl:template match="node()|*|@*">
		<xsl:copy>
			<xsl:apply-templates select="node()|@*" />
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>

  