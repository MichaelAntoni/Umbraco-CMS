<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
    <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library"
  exclude-result-prefixes="msxml umbraco.library">

    <xsl:output method="xml" omit-xml-declaration="yes"/>

    <xsl:param name="currentPage"/>
    <xsl:variable name="imagesPerRow" select="6"/>

    <!-- =============================================================== -->

    <xsl:template match="/">
        <xsl:value-of select="umbraco.library:RegisterStyleSheetFile('StarterkitGallery', '/css/StarterkitGallery.css')"/>

        <!-- a little hack to work around the xslt syntax checker when opening/closing UL lists in two separate IF statements -->
        <!-- if it weren't for IE we could use a single UL and simply float the LI's and they'd auto-wrap -->
        <!-- but for IE we either need separate UL rows or else we need to hard-code the LI's height in CSS -->
        <!-- this approach with separate UL rows works in all browsers and row heights -->
      
        
      <ul class="umbGallery umbGalleryAlbums">
        <xsl:for-each select="$currentPage/* [@isDoc]">

            <!-- display each image in the row, with the count of photos in each gallery -->
            <li>
                <a href="{umbraco.library:NiceUrl(@id)}">
                    <div class="photo">  
                         <img src="{concat(substring-before(./umbGalleryPhoto/umbracoFile,'.'), '_thumb.jpg')}"/>
                    </div>
                    <span class="name">
                        <xsl:value-of select="@nodeName"/>
                    </span>
                </a>
              
                <span class="meta">
                <xsl:value-of select="count(./* [@isDoc])"/>

                <xsl:text> Photo</xsl:text>
                <xsl:if test="count(./* [@isDoc]) &gt; 1">
                    <xsl:text>s</xsl:text>
                </xsl:if>
                </span>
            </li>

        </xsl:for-each>
      </ul>
      
    </xsl:template>

    <!-- =============================================================== -->

</xsl:stylesheet>