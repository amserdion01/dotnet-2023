using Mapster.Common.MemoryMappedTypes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Mapster.Rendering;

public static class TileRenderer
{
    public static BaseShape Tessellate(this MapFeatureData feature, ref BoundingBox boundingBox, ref PriorityQueue<BaseShape, int> shapes)
    {
        BaseShape? baseShape = null;

        var featureType = feature.Type;
        var coordinates = feature.Coordinates;
        // Used from RenderingTypes.
        // Assigning to baseShape a specific Property that is found.
        bool road = Road.isRoad(feature);
        bool water = Waterway.isWaterway(feature);
        bool border = Border.isBorder(feature);
        bool place = PopulatedPlace.isPopulatedPlace(feature);
        bool railway = Railway.isRailway(feature);
        bool natural = GeoFeature.isNatural(feature);
        bool building = GeoFeature.isBuilding(feature);
        bool forest = GeoFeature.isForest(feature);
        bool public_amenity = GeoFeature.isPublicAmenity(feature);
        bool private_amenity = GeoFeature.isPrivateAmenity(feature);
        bool landuseForestOrOrchad = GeoFeature.isLanduseForestOrOrchad(feature);
        bool landusePlain = GeoFeature.isLandusePlain(feature);
        bool landuseResidential = GeoFeature.isLanduseResidential(feature);
        BaseShape temp = null;
        switch (true)
        {

            case var value when value == natural:
                temp = new GeoFeature(coordinates, feature);
                baseShape = temp; 
                break;

            case var value when value == railway:
                temp = new Railway(coordinates);
                baseShape = temp; 
                break;

            case var value when value == road:
                temp = new Road(coordinates);
                baseShape = temp; 
                break;

            case var value when value == water:
                temp = new Waterway(coordinates, feature.Type == GeometryType.Polygon);
                baseShape = temp; 
                break;

            case var value when value == border:
                temp = new Border(coordinates);
                baseShape = temp; 
                break;

            case var value when value == place:
                temp = new PopulatedPlace(coordinates, feature);
                baseShape = temp; 
                break;

            case var value when value == building:
                temp = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                baseShape = temp; 
                break;

            case var value when value == forest:
                temp = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
                baseShape = temp; 
                break;

            case var value when value == public_amenity:
                temp = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Unknown);
                baseShape = temp; 
                break;

            case var value when value == private_amenity:
                temp = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Unknown);
                baseShape = temp; 
                break;

            case var value when value == landuseForestOrOrchad:
                temp = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
                baseShape = temp; 
                break;

            case var value when value == landusePlain:
                temp = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Plain);
                baseShape = temp; 
                break;

            case var value when value == landuseResidential:
                temp = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                baseShape = temp; 
                break;
        }

        if (baseShape != null)
        {
            // Adding the found Property to the priority queue.
            shapes.Enqueue(baseShape, baseShape.ZIndex);

            for (var j = 0; j < baseShape.ScreenCoordinates.Length; ++j)
            {
                boundingBox.MinX = Math.Min(boundingBox.MinX, baseShape.ScreenCoordinates[j].X);
                boundingBox.MaxX = Math.Max(boundingBox.MaxX, baseShape.ScreenCoordinates[j].X);
                boundingBox.MinY = Math.Min(boundingBox.MinY, baseShape.ScreenCoordinates[j].Y);
                boundingBox.MaxY = Math.Max(boundingBox.MaxY, baseShape.ScreenCoordinates[j].Y);
            }
        }

        return baseShape;
    }

    public static Image<Rgba32> Render(this PriorityQueue<BaseShape, int> shapes, BoundingBox boundingBox, int width, int height)
    {
        var canvas = new Image<Rgba32>(width, height);

        // Calculate the scale for each pixel, essentially applying a normalization
        var scaleX = canvas.Width / (boundingBox.MaxX - boundingBox.MinX);
        var scaleY = canvas.Height / (boundingBox.MaxY - boundingBox.MinY);
        var scale = Math.Min(scaleX, scaleY);

        // Background Fill
        canvas.Mutate(x => x.Fill(Color.White));
        while (shapes.Count > 0)
        {
            var entry = shapes.Dequeue();
            entry.TranslateAndScale(boundingBox.MinX, boundingBox.MinY, scale, canvas.Height);
            canvas.Mutate(x => entry.Render(x));
        }

        return canvas;
    }

    public struct BoundingBox
    {
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
    }
}