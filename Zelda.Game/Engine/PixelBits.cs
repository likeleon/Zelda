using System;
using System.Collections.Generic;
using System.Linq;

namespace Zelda.Game.Engine
{
    class PixelBits
    {
        readonly int _width;
        readonly int _height;
        readonly int _numIntegersPerRow;
        readonly uint[][] _bits;

        public PixelBits(Surface surface, Rectangle imagePosition)
        {
            Debug.CheckAssertion(surface._internalSurface != null,
                "Attempt to read a surface that doesn't have pixel buffer in RAM.");

            Rectangle clippedImagePosition = imagePosition.GetIntersection(
                new Rectangle(new Point(0, 0), surface.Size));

            if (clippedImagePosition.IsFlat)
                return;

            _width = clippedImagePosition.Width;
            _height = clippedImagePosition.Height;
            _numIntegersPerRow = _width >> 5; // _width / 32
            if ((_width & 31) != 0)   // _width % 32 != 0
                ++_numIntegersPerRow;

            int pixelIndex = clippedImagePosition.Y * surface.Width + clippedImagePosition.X;
            
            _bits = new uint[_height][];
            for (int i = 0; i < _height; ++i)
            {
                _bits[i] = new uint[_numIntegersPerRow];

                int k = -1;
                uint mask = 0x00000000;
                for (int j = 0; j < _width; ++j)
                {
                    if (mask == 0x00000000)
                    {
                        ++k;
                        mask = 0x80000000;
                        _bits[i][k] = 0x0000000;
                    }

                    if (!surface.IsPixelTransparent(pixelIndex))
                        _bits[i][k] |= mask;

                    mask >>= 1;
                    ++pixelIndex;
                }
                pixelIndex += surface.Width - _width;
            }
        }

        public bool TestCollision(PixelBits other, Point location1, Point location2)
        {
            bool debugPixelCollisions = false;

            if (_bits == null)
                return false;

            // 두 바운딩 박스를 계싼합니다.
            Rectangle boundingBox1 = new Rectangle(location1.X, location1.Y, _width, _height);
            Rectangle boundingBox2 = new Rectangle(location2.X, location2.Y, other._width, other._height);

            // 두 바운딩 박스간 충돌을 체크합니다.
            if (!boundingBox1.Overlaps(boundingBox2))
                return false;

            if (debugPixelCollisions)
            {
                Console.WriteLine("{0}".F(EngineSystem.Now));
                Console.WriteLine(" bounding box collision");
                Console.WriteLine("rect1 = {0}".F(boundingBox1));
                Console.WriteLine("rect2 = {0}".F(boundingBox2));
                Print();
                other.Print();
            }

            // 두 박스 사이의 겹치는 영역을 계산합니다.
            int intersectionX = Math.Max(boundingBox1.X, boundingBox2.X);
            int intersectionY = Math.Max(boundingBox1.Y, boundingBox2.Y);
            Rectangle intersection = new Rectangle(
                intersectionX,
                intersectionY,
                Math.Min(boundingBox1.X + boundingBox1.Width, boundingBox2.X + boundingBox2.Width - intersectionX),
                Math.Min(boundingBox1.Y + boundingBox1.Height, boundingBox2.Y + boundingBox2.Height - intersectionY));

            if (debugPixelCollisions)
                Console.WriteLine("intersection: {0}".F(intersection));

            // 겹치는 영역으로부터 각 바운딩 박스에 상대적인 위치를 계산합니다.
            Point offset1 = intersection.XY - boundingBox1.XY;
            Point offset2 = intersection.XY - boundingBox2.XY;

            if (debugPixelCollisions)
            {
                Console.WriteLine("offset1.x = {0}, offset1.y = {1}, offset2.x = {2}, offset2.y = {3}"
                    .F(offset1.X, offset1.Y, offset2.X, offset2.Y));
            }

            // 겹치는 영역의 각 행에 대해, 오른쪽 바운딩 박스에게는 'a'를, 왼쪽 바운딩 박스에게는 'b' 이름을 사용합니다.
            IEnumerator<uint[]> rowsA, rowsB;
            int numMasksPerRowA, numMasksPerRowB;
            int numUnusedMasksRowB, numUnusedBitsRowB, numUsedBitsRowB;
            
            if (boundingBox1.X > boundingBox2.X)
            {
                rowsA = _bits.Skip(offset1.Y).GetEnumerator();
                rowsB = other._bits.Skip(offset2.Y).GetEnumerator();
                numUnusedMasksRowB = offset2.X >> 5;
                numUnusedBitsRowB = offset2.X & 31;
            }
            else
            {
                rowsA = other._bits.Skip(offset2.Y).GetEnumerator();
                rowsB = _bits.Skip(offset1.Y).GetEnumerator();
                numUnusedMasksRowB = offset1.X >> 5;
                numUnusedBitsRowB = offset1.X & 31;
            }
            numUsedBitsRowB = 32 - numUnusedBitsRowB;

            // a행에서의 겹치는 영역의 마스크 갯수를 계산합니다.
            numMasksPerRowA = intersection.Width >> 5;
            if ((intersection.Width & 31) != 0)
                ++numMasksPerRowA;

            // b행에서의 겹치는 영역의 마스크 갯수를 계산합니다.
            numMasksPerRowB = (intersection.Width + numUnusedBitsRowB) >> 5;
            if (((intersection.Width + numUnusedBitsRowB) & 31) != 0)
                ++numMasksPerRowB;

            // 겹치는 영역의 각 행에 대해서 충돌을 체크합니다
            rowsA.MoveNext();
            rowsB.MoveNext();
            for (int i = 0; i < intersection.Height; ++i)
            {
                IEnumerator<uint> bitsA = ((IEnumerable<uint>)rowsA.Current).GetEnumerator();
                IEnumerator<uint> bitsB = (IEnumerator<uint>)rowsB.Current.Skip(numUnusedMasksRowB).GetEnumerator();

                rowsA.MoveNext();
                rowsB.MoveNext();

                if (debugPixelCollisions)
                    Console.WriteLine("*** checking row {0} of the intersection rectangle".F(i));

                // 각 마스크에 대해 체크
                bitsA.MoveNext();
                bitsB.MoveNext();
                for (int j = 0; j < numMasksPerRowA; ++j)
                {
                    uint maskA = bitsA.Current;
                    uint maskB = bitsB.Current;
                    uint maskALeft = maskA >> numUnusedBitsRowB;
                    uint nextMaskBLeft = 0x00000000;
                    if (j + 1 < numMasksPerRowA ||          // 마지막 a 마스크가 아님
                        numMasksPerRowB > numMasksPerRowA)  // 마지막 a 마스크지만 b마스크가 하나 더 있음
                    {
                        // 여전히 겹치는 영역 내에 있습니다: nextMaskBLeft 존재.
                        bitsB.MoveNext();
                        nextMaskBLeft = bitsB.Current >> numUsedBitsRowB;
                    }

                    if (debugPixelCollisions)
                    {
                        Console.Write("mask_a = ");
                        PrintMask(maskA);
                        Console.Write("mask_b = ");
                        PrintMask(maskB);
                        Console.WriteLine();
                    }

                    if (((maskALeft & maskB) | (maskA & nextMaskBLeft)) != 0x00000000)
                        return true;

                    bitsA.MoveNext();
                    bitsB.MoveNext();
                }
            }

            return false;
        }

        void Print()
        {
            Console.WriteLine("Frame size is {0}x{1}".F(_width, _height));
            for (int i = 0; i < _height; ++i)
            {
                int k = -1;
                uint mask = 0x00000000;
                for (int j = 0; j < _width; ++j)
                {
                    if (mask == 0x00000000)
                    {
                        ++k;
                        mask = 0x80000000;
                    }

                    if ((_bits[i][k] & mask) != 0)
                        Console.Write("X");
                    else
                        Console.Write(".");

                    mask >>= 1;
                }
                Console.WriteLine();
            }
        }

        void PrintMask(uint mask)
        {
            for (int i = 0; i < 32; ++i)
            {
                Console.Write(((mask & 0x80000000) != 0x00000000) ? "X" : ".");
                mask <<= 1;
            }
        }
    }
}
