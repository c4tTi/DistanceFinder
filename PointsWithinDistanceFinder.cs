using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MES.Custom.Helper
{
    public class PointsWithinDistanceFinder
    {

        private double _minBoundingLongitude;
        private double _maxBoundingLongitude;
        private double _minBoundingLatitude;
        private double _maxBoundingLatitude;

        private readonly double _lateralMiddle;
        private readonly double _longitudeMiddle;
        private readonly double _radius;

        public PointsWithinDistanceFinder(double radius, double lat1, double long1)
        {
            _radius = radius;
            _lateralMiddle = lat1;
            _longitudeMiddle = long1;
            CalculateBoundingBox(radius, lat1, long1);
        }

        // TODO: Radian convertion depends on input - Order by distance. 
        public double GetDistance(double lateralPos, double longitudePos)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = ToRadians(lateralPos - _lateralMiddle);  // deg2rad below
            var dLon = ToRadians(longitudePos - _longitudeMiddle);

            var mathSinLat = Math.Sin(dLat / 2);
            var mathSinLong = Math.Sin(dLon / 2);
            var a = mathSinLat * mathSinLat +
                Math.Cos(ToRadians(_lateralMiddle)) * Math.Cos(ToRadians(lateralPos)) *
                mathSinLong * mathSinLong;

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public static double ToRadians(double deg)
        {
            return deg * 0.017453292519943295;
        }

        // Should be called once and data needs to be saved.
        /// <summary>
        /// Calculates largest and smallest latitude and longitude values of the bounding circle.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        private void CalculateBoundingBox(double radius, double latitude, double longitude)
        {
            var R = 6371; // Radius of the earth in km

            double radianRadius = radius / R;
            double radianLatitude = ToRadians(latitude);
            double radianLongitude = ToRadians(longitude);
            
            _minBoundingLatitude = radianLatitude - radianRadius;
            _maxBoundingLatitude = radianLatitude + radianRadius;

            double deltaLongitude = Math.Asin((Math.Sin(radianRadius)) / Math.Cos(radianLatitude));

            _minBoundingLongitude = radianLongitude - deltaLongitude;
            _maxBoundingLongitude = radianLongitude + deltaLongitude;

            // Dealing with poles and the 180th Meridian
            // when lat max is bigger than pi/2 then - north pole is within the query circle thus all meridians are within query circle.
            if (_maxBoundingLatitude > Math.PI / 2)
            {
                _maxBoundingLatitude = Math.PI / 2;
                _minBoundingLongitude = -Math.PI;
                _maxBoundingLongitude = Math.PI;
            }
            else if (_minBoundingLatitude < -Math.PI / 2)
            {
                _minBoundingLatitude = -Math.PI / 2;
                _minBoundingLongitude = -Math.PI;
                _maxBoundingLongitude = Math.PI;
            }
            // THIS should be optimized otherwise - great performance loss:
            if (_maxBoundingLongitude > Math.PI || _maxBoundingLongitude < -Math.PI || _minBoundingLongitude > Math.PI || _minBoundingLongitude < -Math.PI)
            {
                _minBoundingLongitude = -Math.PI;
                _maxBoundingLongitude = Math.PI;
            }
        }

        public bool IsCoordinateInBoundingBox(double latitude, double longitude)
        {
            latitude = ToRadians(latitude);
            longitude = ToRadians(longitude);
            return latitude < _maxBoundingLatitude && latitude > _minBoundingLatitude && longitude < _maxBoundingLongitude && longitude > _minBoundingLongitude;
        }
    }
}
