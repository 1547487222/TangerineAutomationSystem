using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.Foundation.Modules.ModuleStations;
using System.Foundation.Modules.TubeRacks;

namespace System.Foundation.Modules.Models
{
    public class ModuleRackCalculator
    {
        public static List<QPosition> ModuleStationPosCalculates(ModuleRackStationData rackData)
        {
            var qPositions = new List<QPosition>();

            if (rackData.Rows <= 0 || rackData.Cols <= 0)
                return qPositions;

            if (rackData.Rowfirst)
            {
                for (int row = 0; row < rackData.Rows; row++)
                {
                    for (int col = 0; col < rackData.Cols; col++)
                    {
                        float posX = rackData.StartPosition.X + col * rackData.SpaceCol;
                        float posY = rackData.StartPosition.Y + row * rackData.SpaceRow;


                        qPositions.Add(new QPosition
                        {
                            X = posX,
                            Y = posY,
                            Angle = rackData.StartPosition.Angle,
                            Depth = rackData.StartPosition.Depth,
                            Z = rackData.StartPosition.Z,
                            Z2 = rackData.StartPosition.Z2,
                            ZPutGetOffset = rackData.StartPosition.ZPutGetOffset
                        });
                    }
                }
            }
            else
            {
                for (int col = 0; col < rackData.Cols; col++)
                {
                    for (int row = 0; row < rackData.Rows; row++)
                    {
                        float posX = rackData.StartPosition.X + col * rackData.SpaceCol;
                        float posY = rackData.StartPosition.Y + row * rackData.SpaceRow;

                        qPositions.Add(new QPosition
                        {
                            X = posX,
                            Y = posY,
                            Angle = rackData.StartPosition.Angle,
                            Depth = rackData.StartPosition.Depth,
                            Z = rackData.StartPosition.Z,
                            Z2 = rackData.StartPosition.Z2,
                            ZPutGetOffset = rackData.StartPosition.ZPutGetOffset
                        });
                    }
                }
            }

            return qPositions;
        }
    }
}
