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
        <!-- for information on configuring the jquery lightbox, go to -->
        <!-- http://leandrovieira.com/projects/jquery/lightbox/ -->

        <!-- replace this with a real jquery reference when the api is ready -->
        <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('jQuery', 'http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.4.4.min.js')"/>
        <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('jQueryLightbox', '/scripts/jquery.lightbox-0.5.min.js')"/>
        <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('runwayGallery', '/scripts/runway.gallery.js')"/>
        <xsl:value-of select="umbraco.library:RegisterStyleSheetFile('jQueryLightboxCss', '/css/jQueryLightbox.css')"/>
        <xsl:value-of select="umbraco.library:RegisterStyleSheetFile('StarterkitGallery', '/css/StarterkitGallery.css')"/>


     <ul class="umbGallery umbGalleryPhotos">
      <xsl:for-each select="$currentPage/* [@isDoc and string(umbracoNaviHide) != '1']">

            <!-- display each image in the row -->
            <li class="photo{position()}">
                <a href="{umbracoFile}" class="lightbox" title="{@nodeName}">
                  <div class="photo">  
                  <img src="{concat(substring-before(umbracoFile,'.'), '_thumb.jpg')}"/>
                  </div>  
                    <span class="name">
                        <xsl:value-of select="@nodeName"/>
                    </span>
                </a>
            </li>

        </xsl:for-each>
      </ul>
      

        <script type="text/javascript">
            $(function() {
            $('ul.umbGallery a.lightbox').lightBox({
            imageLoading: '/css/jQueryLightboxImages/loading.gif',
            imageBlank: '/css/jQueryLightboxImages/blank.gif',
            imageBtnClose: '/css/jQueryLightboxImages/close.gif',
            imageBtnPrev: '/css/jQueryLightboxImages/prev.gif',
            imageBtnNext: '/css/jQueryLightboxImages/next.gif'
            });
            });
        </script>

    </xsl:template>

    <!-- =============================================================== -->

</xsl:stylesheet>