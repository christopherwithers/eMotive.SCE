/* ($Id: corefunctionsRevised.js 1920 2012-11-27 11:42:32Z winterjp $) */

;(function($j) {

    stopAnim = false;
	sliderAnim = false;
	sliderWidth = 710;
	timer = 0;
	expandedImage = false;
	overlayHeight =0;

	/* Thanks to http://www.hardcode.nl/ for providing the following script */
	function drawSocialLinks(oContainers){
			var l,i,socialList = [], socialHtm='';
			var t = $j('h1').eq(1).text();   /*  adjust this to select the title of your article */
			var u = window.location.href;  /*  this selects the link to your article */
			var iconDirectory = 'http://www.birmingham.ac.uk/Images/website/icons/'; /* this is the director containing your icons */
	 
			var socialMedia = [
				{linkText: 'Del-icio-us', icon:'delicious32.png',href:'http://del.icio.us/post?url='+u+'&title='+t},
				{linkText: 'Stumbleupon', icon:'stumbleupon32.png',href:'http://www.stumbleupon.com/submit?url='+u},
				{linkText: 'Facebook', icon:'facebook32.png',href:'http://www.facebook.com/share.php?u='+u}, 
				{linkText: 'Digg', icon:'digg32.png',href:'http://digg.com/submit?phase=2&url='+u+'&title='+t},
				{linkText: 'Twitter', icon:'twitter32.png',href:'http://twitter.com/home?status='+u}, 
				{linkText: 'Google', icon:'google32.png',href:'http://www.google.com/bookmarks/mark?op=edit&bkmk='+u}
			];
	 
			l = socialMedia.length;
			for (i=0; i<l;i++){
				socialList.push('<li><a href="'+socialMedia[i].href+'" title="'+socialMedia[i].linkText+'"><img width="21" height="21" src="'+iconDirectory+socialMedia[i].icon+'" alt="'+socialMedia[i].linkText+'" /></a></li>');
			}
			socialHtm = '<ul><li class="sys_first">Bookmark this</li>'+socialList.join("\n")+'</ul>';
			oContainers.append('<div class="socialMediaContainer">'+socialHtm+'</div>'); 
			}


	function setAccordionSummaryText() {

		$j('div.sys_accordion').each(function() {

			var $jaccordion = $j(this);

			if ($jaccordion.find('div:hidden').length > 0) {
				$jaccordion.find('h2.sys_summary span').html('Open all sections');
			} else {
				$jaccordion.find('h2.sys_summary span').html('Close all sections');
			};
		})
	}


	$j(document).ready(function(){

		// Draw Social Bookmarks //
			drawSocialLinks($j('#bookmarks'));
			
		//Contensis advert fade-away
			$j('.sys_fadeAway a').animate({ opacity: 1.0 }, 3000).fadeOut('slow', function() {
				$j(this).remove()
			});
		
		//Set up the search input box
		$j('#search .sys_textbox').focus(function(){
			if(this.value == 'Search...')
				this.value = '';
		});
		$j('#search .sys_textbox').blur(function(){
			if(this.value == '')
				this.value = 'Search...';
		});
		
		
		$jfeaturesDiv = $j('#features');
		
        //pdf tracker
        $("a.pdftracker").click(function() {
            _gaq.push(["_trackEvent", "PDF Downloads", "Download", $(this).attr("href")]);
        });
        //doc tracker
        $("a.doctracker").click(function() {
            _gaq.push(["_trackEvent", "Word Document Downloads", "Download", $(this).attr("href")]);
        });
		
		/***************************************/
		//Sentence//
		/***************************************/
		
		$jglobalNav = $j('ul#globalNav');
		$jmainNav = $j('div#mainNav');
		$jmainContent = $j('div#mainContent');
		$jwideContent = $j('div#wideContent');
		$jrelatedContent = $j('div#relatedContent');
		$jglobalNav.find('> li > div div').remove();
		$j('div#navWrap').prepend('<div id="overlay"></div>');
		$jglobalNav.find('> li').append('<div class="sys_overlay2"></div>');
		$jglobalNav.find('> li').append('<div class="sys_showPicture"></div>');
		$jshowPicture = $j('div.sys_showPicture');
		$joverlay = $j('#overlay');
		$joverlay2 = $j('div.sys_overlay2');
		$joverlay.hide();
		$joverlay2.hide();
		$jcontent = $j('#content');

		//Calculate the overlay height
		overlayHeight=$jcontent.outerHeight({margin:true});
		mainNavHeight = $jmainNav.outerHeight({margin:true});
		if (mainNavHeight>overlayHeight)
		{
			overlayHeight=mainNavHeight;
		}
		mainContentHeight = $jmainContent.outerHeight({margin:true});
		if (mainContentHeight>overlayHeight)
		{
			overlayHeight=mainContentHeight;
		}
		wideContentHeight = $jwideContent.outerHeight({margin:true});
		if (wideContentHeight>overlayHeight)
		{
			overlayHeight=wideContentHeight;
		}
		relatedContentHeight = $jrelatedContent.outerHeight({margin:true});
		if (relatedContentHeight>overlayHeight)
		{
			overlayHeight=relatedContentHeight;
		}
		homePageHeight = $jcontent.outerHeight({margin:true})+$jfeaturesDiv.outerHeight({margin:true})
		
		if (homePageHeight>overlayHeight)
		{
			overlayHeight=homePageHeight;
		}
		
		$jglobalNav.find('li').bind('click',function(){
			$joverlay.height(overlayHeight);
			$joverlay.fadeIn('fast');
		});

		$jglobalNav.find('> li > ul').hide();
		
		$jglobalNav.delegate('ul#globalNav > li','click', navAppear);
			
		$jglobalNav.delegate('ul#globalNav ul > li', 'click', function(){
			return false;
		});
			
		$jglobalNav.delegate('ul#globalNav ul > li a', 'click', function(){ 
				if(!$j(this).hasClass('sys_expand')) { 
					window.location = $j(this).attr("href"); 
					return true;
			}
			});
		
		//Events to hide nav if you leave and not hide it if you return
		$jglobalNav.delegate('ul#globalNav > li > ul', 'mouseenter', navEnter);
		$jglobalNav.delegate('ul#globalNav > li > ul', 'mouseleave', navLeave);
		
		//Click events to make the navigation go away if the background is clicked
		$joverlay.bind('click', navClear);
		$joverlay2.bind('click', navClear);
		$jglobalNav.delegate('ul#globalNav ul.sys_grid', 'click', navClear);
		
		//Function to expand the images in the navigation if clicked
		$jglobalNav.delegate('ul#globalNav a.sys_expand', 'click', function() {
			expandedImage = true;
			$jelem = $j(this);
			$jnext = $jelem.next();
			width = $jnext.find('img').width();
			$jelem.parents('li').eq(1).find('div.sys_overlay2').show();
			$jshowPic = $jelem.parents('li').eq(1).find('div.sys_showPicture');
			$jshowPic.html($jnext.html());
			$jshowPic.css({ 'left': (945 / 2) - (width / 2) }); //centre the image

			$jshowPic.show('normal', function() {
					$jshowPic.find('p').css('width', ($j(this).width() - 22) + 'px');
			});
			
			$jshowPic.find('a.sys_collapse').click(collapseImage);
			return false;
		})

		//Functions to retrieve the sentence content if clicked
		$jglobalNav.find('> li').bind('click',function(){
			gridNo = 0;		
			switch ($j(this).attr('id')) {
				case 'navUniversity':
					gridNo = 1;
					break;
				case 'navCommunity':
					gridNo = 2;
						break;
				case 'navStudents':
					gridNo = 3;
						break;
				case 'navPartners':
					gridNo = 4;
					break;
				case 'navAlumni':
					gridNo = 5;
					break;
				case 'navStaff':
					gridNo = 6;
					break;
				case 'navResearch':
					gridNo = 7;
					break;
				case 'navInternational':
					gridNo = 8;
					break;  	
				default:
					break;
			
			}
		/*	if (gridNo != 0) {
			    gridUrl = 'http://www.birmingham.ac.uk/webteam/sentence/grid' + gridNo + '.aspx';
				gridSpan = '#grid' + gridNo;
				gridElement = 'ul#aGrid' + gridNo;
			
				if($j(gridSpan).get(0)){
					$j.ajax({
						url: gridUrl,
						async:false,
						success: function(data) {
								$j(gridSpan).replaceWith($j(data).find(gridElement));
						}
					});
				}
			
				$j('a.media').media({caption: false});
			}*/
		});

		
		$jglobalNav.delegate('ul#globalNav a.sys_collapse', 'click', collapseImage);
		$jglobalNav.find('> li > ul').addClass('sys_origPos');
		
		/***************************************/
		//Sentence end//
		/***************************************/
			
		
			//Slider tabs
			$jsliderWrapper = $j('div#sliderWrapper');

			$j('div.sys_sliderContent').mouseleave(function() {
				$jelem = $j(this).parent();

				$jelem.stop().css({ 'z-index': 500 });
				sliderAnim = true;
				$jsliderWrapper.animate({ 'width': 45 });
				$jelem.animate({ 'right': 0 - sliderWidth }, function() {
					$jelem.css({ 'z-index': $jelem.data('zindex') });
				sliderAnim = false;
			});
			//$jsliderWrapper.animate({ 'width': 45 });
		});

		$j('a.sys_sliderButton').click(function() {
			if (sliderAnim == false) {
				$jelem = $j(this).parent();
				$jelem.data('zindex', $jelem.css('z-index'));
				$jelem.css({ 'z-index': 500 });
				sliderAnim = true;
				$jsliderWrapper.animate({ 'width': sliderWidth + 90 });
				$jelem.animate({ 'right': 0 }, function() {
					sliderAnim = false;
				});
				//$jsliderWrapper.animate({ 'width': 45 });
			}
		 });
	 
		
		
	//for accordions
		$j('div.sys_accordion h2').each(function() {
		var $jh2 = $j(this);

			if ($jh2.hasClass('sys_summary')) {
				$jh2.append('<span>Open all sections</span>').bind('click',function() {

					if ($j(this).parent().parent().find('div.sys_squeezeBox:hidden').length > 0) {
						$j(this).parent().parent().find('div.sys_squeezeBox:hidden').slideDown('fast');
						$j(this).find('span').html('Close all sections');
						$j(this).parent().parent().find('h2:not(.sys_summary)').addClass('sys_collapse').removeClass('sys_expand');
					}
					else {
						$j(this).parent().parent().find('div.sys_squeezeBox').slideUp('fast');
						$j(this).find('span').html('Open all sections');
						$j(this).parent().parent().find('h2:not(.sys_summary)').removeClass('sys_collapse').addClass('sys_expand');
					};

				});
			} 
			else if (!$jh2.hasClass('sys_normal')) {
				//Only add sliders to headers that aren't marked as normal.
                
                //If the header isn't initialised as open then close it.
                if (!$jh2.hasClass('sys_collapse'))
                {
				    $jh2.next('div').hide();
                    $jh2.addClass('sys_expand');
                }

                //Bind click function to the header:
                $jh2.bind('click',function() {
					if ($j(this).hasClass('sys_expand')) {
						$j(this).addClass('sys_collapse').removeClass('sys_expand').next('div').slideDown('fast', setAccordionSummaryText);
					} else {
						$j(this).removeClass('sys_collapse').addClass('sys_expand').next('div').slideUp('fast', setAccordionSummaryText);
					};
				});
			};
		});
        
        //jackie's small content cupboards
        $j('div.contentCupboardsmall h2').each(function() {
            var $jh2 = $(this);

            if (!$jh2.hasClass('sys_collapse'))
            {
                $jh2.next('div').hide();
                $jh2.addClass('sys_expand');
            }

            //Bind click function to the header:
            $jh2.bind('click',function() {
                if ($j(this).hasClass('sys_expand')) {
                    $j(this).addClass('sys_collapse').removeClass('sys_expand').next('div').slideDown('fast');
                } else {
                    $j(this).removeClass('sys_collapse').addClass('sys_expand').next('div').slideUp('fast');
                };
            });
        });
		
		//Converts media galleries to enable clicking on pictures.
		$j('div.sys_mediagallery-control div.sys_itemslist').each(function() {
			
			$jitemslist = $j(this);
			//Go through each item in the list
			$jitemslist.find('div.sys_subitem div').each(function() {
			
				$jitem = $j(this);
				//Get the anchor from inside the item
				$janchor = $jitem.find('h3 a');

				//Get the name of the picture and remove it from the anchor
				nameOfPicture = $janchor.html();
				$janchor.html('');

				//Wrap the anchor around the div for the item
				$jitem.wrap($janchor);

				//Find the original anchor and replace it with a span, putting in the name of the picture
				$jitem.find('h3 a').replaceWith(function(){
					return $j("<span>" + nameOfPicture+ "</span>");
				});
			});
		});

        openSection = getQueryStringValue('OpenSection');
        if (openSection !=null)
        {
            $j.scrollTo('#' + openSection, 1000);
        }

	});

	/***************************************/
	//Sentence functions//
	/***************************************/
	function trans1($jelem) {
		$jelem.fadeIn('fast',function(){
			$j(this).next().length && !stopAnim && trans1($j(this).next()); 
		});
	}

	function navAppear() {
		$jglobalNav.find('li.sys_iehover').removeClass('sys_iehover');
		$jglobalNav.find('> li > ul').hide();
		stopAnim = false;
		//$jglobalNav.find('> li').unbind('mouseenter');
		$jelem = $j(this);
		$jelem.addClass('sys_iehover');
		$jelem.find('> ul').eq(1).show();
		$jelem.find('> ul').eq(1).find('> li').hide();
		$jelem.find('> ul').eq(0).fadeIn('fast', function() {
			if (!stopAnim) {
				$jitems = $jelem.find('> ul:last > li');
				trans1($jitems.eq(0));
			}
		});
	}

	function navEnter() {
		clearTimeout(timer);
	}

	function navLeave() {
		if (expandedImage == false) {
			$jelem = $j(this).parent();
			timer = setTimeout(function() {
				navHide($jelem);
			}, 500);
		}
	}

	function navClear()
	{
		if (expandedImage) {
			collapseImage();
		}
		//Find the current open navigation
		$jelem = $jglobalNav.find('li.sys_iehover');
		navHide($jelem);
	}

	function navHide($jelem) {
		stopAnim = true;
		// $globalNav.find('> li').bind('mouseenter', navAppear);
		$juls = $jelem.find('> ul');
		$juls.fadeOut('slow', function() {
			$jelem.removeClass('sys_iehover');
		});
		$joverlay.fadeOut('slow');
		$joverlay2.fadeOut('slow');
		$jshowPicture.fadeOut('slow');
	}

	function collapseImage() {
		expandedImage = false;
		$joverlay2.hide();
		$jshowPicture.hide('normal');
		return false;
	}

	/***************************************/
	//Sentence functions end//
	/***************************************/

	/* used to get rid of _blank so your code validates w3c stylee */
	function setExternalLinks() {
	  var el_list = document.getElementsByTagName('A');
	  for (i=0; i<el_list.length; i++) {
		if (el_list[i].getAttribute('rel') == 'external') {
		  el_list[i].setAttribute('target', '_blank');
		}
	  }
	}

	/* Check's users version of Flash */
	function checkFlash (version) {
		return DetectFlashVer(version, 0, 0);
	}

    function getQueryStringValue(name) {
        name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regexS = "[\\?&]" + name + "=([^&#]*)";
        var regex = new RegExp(regexS);
        var results = regex.exec(window.location.search);
        if(results == null)
            return null;
        else
            return decodeURIComponent(results[1].replace(/\+/g, " "));
    }

})(jQuery);