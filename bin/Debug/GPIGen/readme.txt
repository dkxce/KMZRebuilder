GPIgen by Lukas Gebauer (Czech Republic)
  Translating set of GPX files into one GPI file.

Usage: gpigen.exe <source_path> <destination_file>

Features:
- Can process GPX files in same structure as for Garmin (tm) PoiLoader.
- GPX files using same internal format as for Garmin (tm) PoiLoader.
- Can process multiple POI groups with multiple categories.

- POI icons can be defined by a category or for each POI separately.

- Any POI can have attached JPEG image.
- POI can have comment and/or description (include HTML).
- POI can have an address fields.
- POI can have a contact fields.
- Proximity and speed alerts are supported.
- Support for generate POI in unicode.

Notes:
- supported POI icons are 8-bit uncompressed BMP up to 48x48 size only!
- CSV sources are not supported! You must use GPX only.
- TourGuide audio is not supported.


Enhanced features:
You can put gpigen.ini into your source_path for enhanced feature control.
Content of gpigen.ini can contain:

[gpigen]
codepage=65001
- enable output in unicode. (UTF-8 supported by Oregon, Dakota, Etrex20/30) 
Default is your system ANSI codepage, like PoiLoader does.)

maxregion=256
- set maximum POI in one internal region. Large number reduce GPS performance, 
too many regions reduce perfomance too.  
