// author: dkxce@mail.ru //
		
		Math.sinh = Math.sinh || function(x) 
		{
			var y = Math.exp(x);
			return (y - 1 / y) / 2;
		}
		
		function MapMerger(wi, he)
		{
			this.width = wi;
			this.height = he;
			
			//this.url = 'http://mts0.google.com/vt/lyrs=m@177000000&hl=ru&src=app&x={x}&s=&y={y}&z={z}&s=Ga'; // Google
			this.url = 'http://tile.openstreetmap.org/{z}/{x}/{y}.png'; // Mapnik
			//this.url = 'http://tile.xn--pnvkarte-m4a.de/tilegen/{z}/{x}/{y}.png'; // Opnvkarte
			
			this.iconSymbol = '/>';
			this.iconAngle = 0;
			this.iconTitle = '';
			this.iconHREF = '#';
		};
		
		MapMerger.prototype.InitIcon = function(symbol, angle, title, href)
		{
			this.iconSymbol = symbol;
			this.iconAngle = angle;
			this.iconTitle = title;
			this.iconHREF = href;
		};
		
		MapMerger.prototype.GetTileXYFromLatLon = function(lat, lon, zoom)
		{
			var x = Math.floor((lon + 180.0) / 360.0 * Math.pow(2.0, zoom));
			var y = Math.floor((1.0 - Math.log(Math.tan(lat * Math.PI / 180.0) + 1.0 / Math.cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * Math.pow(2.0, zoom));
			return {'x':x,'y':y};
		};
		
		MapMerger.prototype.GetLatLonFromTileXY = function(x, y, zoom)
		{
			var lon = ((x / Math.pow(2.0, zoom) * 360.0) - 180.0);
			var n = Math.PI - ((2.0 * Math.PI * y) / Math.pow(2.0, zoom));
			var lat = (180.0 / Math.PI * Math.atan(Math.sinh(n)));
			return {'lat':lat,'lon':lon};
		};
		
		MapMerger.prototype.GetTile = function (x, y, z) 
		{			
			return this.url.replace('{x}',x).replace('{y}',y).replace('{z}',z);
		};
		
		MapMerger.prototype.SymbolToImage = function(symb)
		{
			var prose = 'primary';
			if(symb.length == 2)
			{
				if(symb[0] == '\\') prose = 'secondary';
				symb = symb.substr(1);
			};
			var symbtable = '!"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~';						
			var idd = symbtable.indexOf(symb);
			if(idd < 0) idd = 14;
			var itop =  Math.floor(idd / 16) * 24;
			var ileft = (idd % 16) * 24;
			return 'background:url(../v/images/'+prose+'.png) -'+ileft+'px -'+itop+'px no-repeat;';
		}
		
		MapMerger.prototype.CalcGeo = function ()
		{			
			var center = this.GetTileXYFromLatLon(this.mLat, this.mLon, this.mZoom);
			this.Center_TileX = center.x;
			this.Center_TileY = center.y;
			
			var topleft = this.GetLatLonFromTileXY(center.x, center.y, this.mZoom);
			var bottomright = this.GetLatLonFromTileXY(center.x + 1, center.y + 1, this.mZoom);
			
			this.LonPerTile = 360.0 / (Math.pow(2, this.mZoom));
            this.LonPerPixel = this.LonPerTile / 256;
			
			this.Center_Tile_XPos = Math.floor(this.width / 2 - (256.0 * ((this.mLon - topleft.lon) / (bottomright.lon - topleft.lon))));			
			this.Center_Tile_YPos = Math.floor(this.height / 2 - (256.0 * ((this.mLat - topleft.lat) / (bottomright.lat - topleft.lat))));			
		};
		
		MapMerger.prototype.GetMap = function(lat, lon, zoom)
		{
			this.mLat = lat;
			this.mLon = lon;
			this.mZoom = zoom;
			this.CalcGeo();
		
			var ddv = '<div style="display:inline-block;width:'+this.width+';height:'+this.height+';overflow:hidden;border:solid 1px gray;">';
			
			var drawx = this.Center_Tile_XPos;		
			var drawy = this.Center_Tile_YPos;
			
			var x = this.Center_TileX;
			var y = this.Center_TileY;
			
			while (drawx >= 0) { drawx -= 256; x--; };
			while (drawy >= 0) { drawy -= 256; y--; };
			
			var curdrawx;
			var curx;
			while (drawy < this.height)
            {
				curdrawx = drawx;
				curx = x;
				while (curdrawx < this.width)
				{
					try {
						var url = this.GetTile(curx, y, this.mZoom);
						ddv += '<div style="display:inline-block;width:'+this.width+';height:'+this.height+';;overflow:hidden;background:url('+url+') '+curdrawx+'px '+drawy+'px no-repeat;position:absolute;"></div>';
					} catch (ex) { };
					curdrawx += 256;
					curx++;
				};
				drawy += 256;
				y++;
            };			
			
			// place icon //
			var smb = this.SymbolToImage(this.iconSymbol);
			var title = "Icon";
			ddv += '<div style="display:block;width:24;height:24;overflow:hidden;'+smb+';position:relative;left:'+(this.width/2-12)+';top:'+(this.height/2-12)+';"></div>';
			ddv += '<div style="display:block;width:32;height:32;overflow:visible;position:relative;left:'+(this.width/2-16)+';top:'+(this.height/2-16-24)+';"><a href="'+this.iconHREF+'" target="_blank"><img src="../v/images/arrow.png" style="transform:rotate('+this.iconAngle+'deg);transform-origin:50% 50% 0px;" title="'+this.iconTitle+'"/></a></div>';
			// -- //
			
			ddv += '</div>';
			return ddv;
		};