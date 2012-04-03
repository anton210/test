<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
                exclude-result-prefixes="msxsl xxx" 
                xmlns:xd="http://schemas.microsoft.com/office/infopath/2003" 
                xmlns:ext="http://mcdean"
                xmlns:xxx="http://mcderan/customxsl"
                xmlns:my="http://schemas.microsoft.com/office/infopath/2003/myXSD/2011-07-27T21:37:51" >
  <!--xmlns:my="http://schemas.microsoft.com/office/infopath/2003/myXSD/2011-01-20T04:39:26" -->
	<xsl:output method="xml" indent="yes"/>
  <xsl:namespace-alias stylesheet-prefix="xxx" result-prefix="xsl"/>
	<xsl:param name="ScreenPadding" select="4"/>
	<xsl:param name="ScreenWidth" select="768"/>
  <xsl:param name="LeaveStylesList" select="'width|height|border|border-bottom|border-top|border-left|border-right|border-bottom-color|border-bottom-left-radius|border-bottom-right-radius|border-bottom-style|border-bottom-width|border-left-color| border-left-style| border-left-width| border-right-color|border-right-style|border-right-width|border-top-color| border-top-left-radius|border-top-right-radius|border-top-style|border-top-width|vertical-align|overflow-x|overflow-y'"/>

  <xsl:template match="xsl:output">
    <xxx:output method="html" indent="yes"/>
  </xsl:template>
  
  <!-- Initialize global variables -->
  <xsl:variable name="ComponentsWidth">
    <root>
      <xsl:for-each select="//*[contains(@class,'xdLayout')] | //*[contains(@class,'xdSection')]">
        <xsl:if test="@style">
          <a>
            <xsl:call-template name="GetWidthFromString">
              <xsl:with-param name="strWidth" select="ext:GetStyle(@style, 'width')"/>
            </xsl:call-template>
          </a>
        </xsl:if>
      </xsl:for-each>
    </root>
  </xsl:variable>

  <xsl:variable name="MaxWidth">
    <xsl:for-each select="msxsl:node-set($ComponentsWidth)/root/a">
      <xsl:sort order="descending" select="."/>
      <xsl:if test="position()=1">
        <xsl:value-of select="."/>
      </xsl:if>
    </xsl:for-each>
  </xsl:variable>

  <xsl:variable name="WidthCorrection" select="($ScreenWidth - (2 * $ScreenPadding)) div $MaxWidth"/>
  
  <!-- Auxiliary templates-->
  <xsl:template name="GetWidthFromString">
    <xsl:param name="strWidth"/>
    <xsl:choose>
      <xsl:when test="contains($strWidth, 'px')">
        <xsl:value-of select="number(substring-before($strWidth, 'px'))"/>
      </xsl:when>
      <xsl:when test="contains($strWidth, 'pt')">
        <xsl:value-of select="number(substring-before($strWidth, 'pt'))  * 4 div 3"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'0'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <!-- Second Pass templates-->

  <xsl:template match="@style" mode="SecondPass">
    <xsl:variable name="NewValue">
      <xsl:variable name="RawWidth" select="ext:GetStyle(., 'width')"/>
      <xsl:choose>
        <xsl:when test="ext:Contains($RawWidth, 'px') or ext:Contains($RawWidth, 'pt')">
          <xsl:variable name="WidthInPx">
            <xsl:call-template name="GetWidthFromString">
              <xsl:with-param name="strWidth" select="ext:GetStyle(., 'width')"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:value-of select="ext:SetStyle(., 'width', concat(number($WidthInPx) * $WidthCorrection, 'px'))"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="."/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:attribute name="style">
      <xsl:value-of select="ext:CleanStyles($NewValue, $LeaveStylesList)"/>
    </xsl:attribute>
  </xsl:template>
  

  <!-- Remove Attributes-->
  <xsl:template match="@size | @face | @color" mode="SecondPass"/>
  
  <xsl:template match="@*" mode="SecondPass">
     <xsl:copy/>
  </xsl:template>

  <xsl:template match="node()" mode="SecondPass">
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="SecondPass"/>
      <!--xsl:copy-of select="@*"/-->
      <xsl:apply-templates select="node()" mode="SecondPass"/>
    </xsl:copy>
  </xsl:template>

  <!-- First Pass Templates -->
	<xsl:template match="@* | node()">
		<xsl:copy>
			<xsl:apply-templates select="@* | node()"/>
		</xsl:copy>
	</xsl:template>

  <xsl:template match="/">
    <!-- Create a variable which contains transformation result after first pass-->
    <xsl:variable name="TempDoc">
      <xsl:apply-templates />
    </xsl:variable>

   <!-- Apply second pass templates-->
   <xsl:apply-templates select="msxsl:node-set($TempDoc)/xsl:stylesheet" mode="SecondPass"/>
  </xsl:template>
  
	<xsl:template match="node()" mode="childcontrol" priority="-1">
		<xsl:apply-templates select="."/>
	</xsl:template>

	<!-- ApplyCoreLayoutMods -->
  <xsl:template match="body">
    <body>
      <div data-role="page" id="mainForm">
        <xsl:apply-templates select="./node()"/>
      </div>
      <!-- Create signature pad dialog page -->
      <div data-role="page" class="signature-dialog" id="popup" data-theme="c" data-close-btn-text="Cancel">
        <div data-role="header" data-position="inline">
          <h1>Signature Pad</h1>
          <a id="sigPadDoneButton" href="#" >Done</a>
        </div>
        <div data-role="content">
          <form method="post" class="sigPad" action="#" id="sigDialogPad">
            <div class="sigWrapper">
            </div>
          </form>
        </div>
        <div data-role="footer">
          <a id="sigPadClearButton" href="#" >Clear</a>
        </div>
      </div>
    </body>
	</xsl:template>
	<!-- ApplyHeadTransform -->
	<xsl:template match="head/style | head/meta"/>
	<!-- remove all style and meta tags from head -->
	<xsl:template match="head">

		<xsl:copy>
			<xsl:apply-templates/>
			<meta name="viewport" content="user-scalable=no, width=device-width, initial-scale=1, maximum-scale=1.0"/>
			<meta name="apple-mobile-web-app-capable" content="yes"/>
      <link rel="stylesheet" href="iPadTransform.css"/>
      <link rel="stylesheet" href="http://code.jquery.com/mobile/1.1.0-rc.1/jquery.mobile-1.1.0-rc.1.min.css" />

      <script src="http://code.jquery.com/jquery-1.7.1.min.js"></script>
      <script src="http://code.jquery.com/mobile/1.1.0-rc.1/jquery.mobile-1.1.0-rc.1.min.js"></script>
     
			<script type="text/javascript" src="iPadTransform.js"/>
      <script type="text/javascript" src="Signature.js"/>
		</xsl:copy>
	</xsl:template>
  
  <!-- Create signatures -->
  <!-- Here I assume that signature goes alwyas inside  <xsl:if test="function-available('xdImage:getImageUrl')"> tag.-->
  <xsl:template match="xsl:if[object[@xd:xctname='inkpicture'] or img[@xd:xctname='InlineImage']]">
    <!--xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:apply-templates select="node()" mode="childcontrol"/>
    </xsl:copy-->

    <xsl:variable name="InkPictureNode" select="./object[@xd:xctname='inkpicture'] | ./img[@xd:xctname='InlineImage']"/>
    
    <!--xsl:if test="local-name()='if'"-->
      <a class="signLink" href="#popup" id="{xd:CtrlId}" data-rel="dialog" data-transition="none">
        <input type="hidden" class="signature" xd:binding="{$InkPictureNode/@xd:binding}"/>
        <img class="preview" src="sign-here.gif">
          <xsl:attribute name="style">
            <xsl:value-of select="concat('width:', ext:GetStyle($InkPictureNode/@style, 'width'), ';')"/>
          </xsl:attribute>
        </img>
      </a>
    <!--/xsl:if-->

  </xsl:template>
  
	<!-- ApplyTextboxMods-->

  <xsl:template match="span[@xd:xctname='RichText']">
    <textarea type="text" id="{@xd:CtrlId}" name="{@xd:CtrlId}" xd:binding ="{@xd:binding}">
      <xsl:attribute name="style">
        <xsl:value-of select="ext:SetStyle(ext:SetStyle(@style, 'height', '100%'), 'min-height', '36px')"/>
      </xsl:attribute>
      <xxx:value-of select="{@xd:binding}"/>
    </textarea>
  </xsl:template>
  
	<xsl:template match="node()[span[@xd:xctname='PlainText']]">
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:if test="count(span[@xd:xctname='PlainText' and ext:Contains(@style, 'overflow-y')]) &gt; 0">
				<xsl:attribute name="class">
					<xsl:value-of select="concat(@class, ' textarea-container')"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="./node()" mode="childcontrol"/>
		</xsl:copy>
	</xsl:template>
	<xsl:template match="span" mode="childcontrol">
    <!--xsl:copy-of select="."/-->
		<xsl:choose>
			<xsl:when test="ext:Contains(@style, 'overflow-y')">
				<textarea type="text" id="{@xd:CtrlId}" name="{@xd:CtrlId}" xd:binding ="{@xd:binding}">
					<xsl:attribute name="style">
            <xsl:value-of select="ext:SetStyle(ext:SetStyle(@style, 'height', '100%'), 'min-height', '36px')"/>
          </xsl:attribute>
          <xxx:value-of select="{@xd:binding}"/>
				</textarea>
			</xsl:when>
			<xsl:otherwise>
				<input type="text" id="{@xd:CtrlId}" name="{@xd:CtrlId}" xd:binding ="{@xd:binding}">
					<xsl:attribute name="style">
            <xsl:value-of select="ext:RemoveStyle(@style, 'height')"/>
          </xsl:attribute>
          <xxx:attribute name="value">
            <xxx:value-of select="{@xd:binding}"/>
          </xxx:attribute>
				</input>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!--ApplyRadioButtonMods-->
	<xsl:template match="node()[input[@type='radio']]">
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="./node()" mode="childcontrol"/>
		</xsl:copy>
	</xsl:template>
	<xsl:template match="input[@type='radio']" mode="childcontrol">
    <label>
			<xsl:attribute name="for"><xsl:value-of select="@xd:CtrlId"/></xsl:attribute>
		</label>
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:attribute name="value"><xsl:value-of select="@xd:onValue"/></xsl:attribute>
			<xsl:attribute name="name"><xsl:value-of select="@xd:binding"/></xsl:attribute>
			<xsl:attribute name="id"><xsl:value-of select="@xd:CtrlId"/></xsl:attribute>

      <xxx:if test="{@xd:binding}=&quot;{@xd:onValue}&quot;">
        <xxx:attribute name="checked">checked</xxx:attribute>
      </xxx:if>
      
		</xsl:copy>
	</xsl:template>
	<!-- ApplyCheckboxMods -->
	<xsl:template match="node()[input[@type='checkbox']]">
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
      <xsl:attribute name="class"><xsl:value-of select="'checkbox-wrapper'"/></xsl:attribute>
			<xsl:apply-templates select="./node()" mode="childcontrol"/>
		</xsl:copy>
	</xsl:template>
	<xsl:template match="input[@type='checkbox']" mode="childcontrol">
		<select id="{@xd:CtrlId}" name="{@xd:CtrlId}" data-role="slider">
      <option value="{@xd:onValue}">Yes</option>
			<option value="{@xd:offValue}">No</option>
		</select>
	</xsl:template>
</xsl:stylesheet>
