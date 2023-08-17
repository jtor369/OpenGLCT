using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial8
{
    class Program
    {
        private static int width = 1280, height = 720;
        private static ShaderProgram program;
        private static Dictionary<int,VBO<Vector3>> projections;
        private static Dictionary<int, VBO<Vector4>> projectionsUV;
        private static Dictionary<int, DetectorObject> detectorPositions;
        private static Dictionary<int, Vector3D> sourcePositions;

        private static VBO<Vector3> cube, cubeNormals;
        //private static VBO<Vector2> cubeUV;
        private static VBO<Vector4> cubeUV;
        
        private static VBO<int> cubeQuads;
        private static Texture glassTexture;
        private static Dictionary<int,Texture> textures;
        private static System.Diagnostics.Stopwatch watch;
        private static float xangle, yangle;
        private static bool autoRotate, lighting = false, fullscreen = false, alpha = true;
        private static bool add = true;
        private static bool left, right, up, down;
        private static bool advance, retreat;


        private static bool rleft;
        private static bool rright;
        private static bool lleft;
        private static bool lright;
        private static bool forward;
        private static bool backward;

        public static double orthoProjectionCoefficient(Line3D line, Vector3D point)
        {
            Vector3D v = line.directionRaw;
            Vector3D s = point.Subtract(line.offset);

            return v.Dot(s) / s.Dot(s);
        }

        private static Vector3D sourceVector;
        private static DetectorObject detector;
        private static Plane3D projectionPlane;
        private static double projectionY = -0.9999;
        private static double projectionYincrement = 0.02;

        private static double rotateMixA = 1;
        private static IntersectInfo iInfo;

        public static void updateProjectionPlane()
        {
            Vector3D planeNormal = new Vector3D(0.0, rotateMixA, 1 - rotateMixA);
            projectionPlane = new Plane3D(new Line3D(planeNormal, planeNormal.MultiplyScalar(projectionY)));
            //projectionPlane = new Plane3D(new Line3D(planeNormal, new Vector3D(0,projectionY,0)));
        }

        public static Vector3 convertVector3DtoVector3(Vector3D vector)
        {
            //return new Vector3(vector.x,);

            return new Vector3(vector.x, vector.y, vector.z);
        }

        public static Vector2 convertTupleToVector2(Tuple<double, double> vector)
        {
            return new Vector2(vector.Item1, vector.Item2);
        }

        public static void updateIntersectionInfo()
        {
            foreach (var i in sourcePositions.Keys)
            {
                var tempDetector = detectorPositions[i];
                var tempSource = sourcePositions[i];
                iInfo = InitProjections.calculateTextureProjection(tempSource, tempDetector, projectionPlane);
                projections[i] = new VBO<Vector3>(new Vector3[]
                {
                    convertVector3DtoVector3(iInfo.bottomLeftIntersect), convertVector3DtoVector3(iInfo.bottomRightIntersect), convertVector3DtoVector3(iInfo.topRightIntersect), convertVector3DtoVector3(iInfo.topLeftIntersect),
                    convertVector3DtoVector3(tempDetector.BottomLeft), convertVector3DtoVector3(tempDetector.BottomRight), convertVector3DtoVector3(tempDetector.TopRight), convertVector3DtoVector3(tempDetector.TopLeft)
                });
                float m = (float)iInfo.topRightIntersect.Subtract(iInfo.topLeftIntersect).Abs() / 2;
                float n = (float)iInfo.bottomRightIntersect.Subtract(iInfo.bottomLeftIntersect).Abs() / 2;

                float k = (float)tempDetector.TopRight.Subtract(tempDetector.TopLeft).Abs() / 2;
                float u = (float)tempDetector.BottomRight.Subtract(tempDetector.BottomLeft).Abs() / 2;

                projectionsUV[i] = new VBO<Vector4>(new Vector4[]
                {
                    //new Vector4(   convertTupleToVector2(iInfo.bottomLeftCrop),0,1/(float)iInfo.bottomRightIntersect.Subtract(iInfo.bottomLeftIntersect).Abs()),
                    new Vector4(   0,iInfo.bottomLeftCrop.Item2*n,0,n),
                    new Vector4(   n,iInfo.bottomRightCrop.Item2*n,0,n),
                    new Vector4(   m,iInfo.topRightCrop.Item2*m,0,m),
                    new Vector4(   0,iInfo.topLeftCrop.Item2*m,0,m),

                    new Vector4( 0, 0*u, 0, u),
                    new Vector4( u, 0*u, 0, u),
                    new Vector4( k, 1*k, 0, k),
                    new Vector4( 0, 1*k, 0, k)
                });
            }
            iInfo = InitProjections.calculateTextureProjection(sourceVector, detector, projectionPlane);

            Vector3 planeNormal = convertVector3DtoVector3(projectionPlane.normal);


            //cube = new VBO<Vector3>(new Vector3[] {
            //    new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1)
            //});         // top

            cube = new VBO<Vector3>(new Vector3[]
            {
                convertVector3DtoVector3(iInfo.bottomLeftIntersect), convertVector3DtoVector3(iInfo.bottomRightIntersect), convertVector3DtoVector3(iInfo.topRightIntersect), convertVector3DtoVector3(iInfo.topLeftIntersect),
                convertVector3DtoVector3(detector.BottomLeft), convertVector3DtoVector3(detector.BottomRight), convertVector3DtoVector3(detector.TopRight), convertVector3DtoVector3(detector.TopLeft)
            });


            cubeNormals = new VBO<Vector3>(new Vector3[]
            {
                planeNormal, planeNormal, planeNormal, planeNormal,
                planeNormal, planeNormal, planeNormal, planeNormal
            });
            //cubeUV = new VBO<Vector2>(new Vector2[]
            //{
            //    convertTupleToVector2(iInfo.bottomLeftCrop), convertTupleToVector2(iInfo.bottomRightCrop), convertTupleToVector2(iInfo.topRightCrop), convertTupleToVector2(iInfo.topLeftCrop),
            //    new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
            //});
            //float m = (float)iInfo.topRightIntersect.Subtract(iInfo.topLeftIntersect).Abs() / 2;
            //float n = (float)iInfo.bottomRightIntersect.Subtract(iInfo.bottomLeftIntersect).Abs() / 2;

            //float k = (float)detector.TopRight.Subtract(detector.TopLeft).Abs() / 2;
            //float u = (float)detector.BottomRight.Subtract(detector.BottomLeft).Abs() / 2;

            //cubeUV = new VBO<Vector4>(new Vector4[]
            // {
            //     //new Vector4(   convertTupleToVector2(iInfo.bottomLeftCrop),0,1/(float)iInfo.bottomRightIntersect.Subtract(iInfo.bottomLeftIntersect).Abs()),
            //     new Vector4(   0,iInfo.bottomLeftCrop.Item2*n,0,n),
            //    new Vector4(   n,iInfo.bottomRightCrop.Item2*n,0,n),
            //    new Vector4(   m,iInfo.topRightCrop.Item2*m,0,m),
            //    new Vector4(   0,iInfo.topLeftCrop.Item2*m,0,m),

            //    new Vector4( 0, 0*u, 0, u),
            //    new Vector4( u, 0*u, 0, u),
            //    new Vector4( k, 1*k, 0, k),
            //    new Vector4( 0, 1*k, 0, k)
            // });


            cubeQuads = new VBO<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, BufferTarget.ElementArrayBuffer);
        }


        public static MessageWindow mw;

        static void Main(string[] args)
        {



            string text = System.IO.File.ReadAllText(@"processedVals.json");
            //string text = System.IO.File.ReadAllText(@"pVals.json");

            JObject json = JObject.Parse(text);


            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("OpenGL Tutorial");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);

            Glut.glutKeyboardFunc(OnKeyboardDown);
            Glut.glutKeyboardUpFunc(OnKeyboardUp);

            Glut.glutCloseFunc(OnClose);
            Glut.glutReshapeFunc(OnReshape);

            Gl.Disable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            //Gl.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            //Gl.glHint(GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST);
            program = new ShaderProgram(VertexShader, FragmentShader);

            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.6f, (float)width / height, 0.1f, 1000f));
            //program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(1f, (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 100), Vector3.Zero, new Vector3(0, 1, 0)));

            program["light_direction"].SetValue(new Vector3(0, 0, 1));
            program["enable_lighting"].SetValue(lighting);

            //glassTexture = new Texture("y.tif");
            sourceVector = new Vector3D(0, 0, 2);


            detector =
                new DetectorObject(
                    new Vector3D(-1, 1, -1),
                    new Vector3D(1, 1, -1),
                    new Vector3D(-1, -1, -1),
                    new Vector3D(1, -1, -1)
                );
            textures = new Dictionary<int, Texture>();
            detectorPositions = new Dictionary<int, DetectorObject>();
            projections = new Dictionary<int, VBO<Vector3>>();
            projectionsUV = new Dictionary<int, VBO<Vector4>>();
            sourcePositions = new Dictionary<int, Vector3D>();


            double scf = 0.1;
            foreach (var val in json)
            {
                string index = val.Key;
                int i = int.Parse(index);
                string fn = $"{index}.png";
                //string fn = "x.tif";
                textures[i] = new Texture(fn);
                var dTR_json = json[index]["dTR"];
                var dTL_json = json[index]["dTL"];
                var dBR_json = json[index]["dBR"];
                var dBL_json = json[index]["dBL"];

                detectorPositions[i] = new DetectorObject(
                    new Vector3D((double)dTL_json[0] * scf, (double)dTL_json[1] * scf, (double)dTL_json[2] * scf), 
                    new Vector3D((double)dTR_json[0] * scf, (double)dTR_json[1] * scf, (double)dTR_json[2] * scf), 
                    new Vector3D((double)dBL_json[0] * scf, (double)dBL_json[1] * scf, (double)dBL_json[2] * scf), 
                    new Vector3D((double)dBR_json[0] * scf, (double)dBR_json[1] * scf, (double)dBR_json[2] * scf) 
                    );

                var sPos_json = json[index]["sPos"];
                sourcePositions[i] = new Vector3D((double)sPos_json[0] * scf, (double)sPos_json[1] * scf, (double)sPos_json[2] * scf);
            }
            glassTexture2 = new Texture("checker.png");
            glassTexture = new Texture("test2.png");
            mw = new MessageWindow();
            mw.Show();


            updateProjectionPlane();
            updateIntersectionInfo();
          


            watch = System.Diagnostics.Stopwatch.StartNew();




            Glut.glutMainLoop();
        }

        private static void OnClose()
        {
            cube.Dispose();
            //cubeNormals.Dispose();
            //cubeUV.Dispose();
            cubeQuads.Dispose();
            foreach (var t in textures)
            {
                t.Value.Dispose();
            }
            foreach (var t in projections)
            {
                t.Value.Dispose();
            }
            foreach (var t in projectionsUV)
            {
                t.Value.Dispose();
            }
            glassTexture.Dispose();
            program.DisposeChildren = true;
            program.Dispose();
        }

        private static void OnDisplay()
        {

        }

        private static void OnRenderFrame()
        {
            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();

            // perform rotation of the cube depending on the keyboard state
            if (autoRotate)
            {
                xangle += deltaTime / 2;
                yangle += deltaTime;
            }
            if (right) yangle += deltaTime;
            if (left) yangle -= deltaTime;
            if (up) xangle -= deltaTime;
            if (down) xangle += deltaTime;

            if (advance)
            {
                projectionY += projectionYincrement;
                updateProjectionPlane();
                updateIntersectionInfo();
            }
            else if (retreat)
            {
                projectionY -= projectionYincrement;
                updateProjectionPlane();
                updateIntersectionInfo();
            }

            // set up the viewport and clear the previous depth and color buffers
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // make sure the shader program and texture are being used
            Gl.UseProgram(program);
            //Gl.BindTexture(glassTexture);

            // set up the model matrix and draw the cube
            //program["model_matrix"].SetValue(Matrix4.CreateRotationY(yangle) * Matrix4.CreateRotationX(xangle));
            program["enable_lighting"].SetValue(lighting);

            //Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
            //Gl.BindBufferToShaderAttribute(cubeNormals, program, "vertexNormal");
            //Gl.BindBufferToShaderAttribute(cubeUV, program, "vertexUV");
            //Gl.BindBuffer(cubeQuads);

            //Gl.DrawElements(BeginMode.Quads, cubeQuads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);


            foreach (var i in projections.Keys)
            {
                Gl.BindTexture(textures[i]);
                program["model_matrix"].SetValue(Matrix4.CreateRotationY(yangle) * Matrix4.CreateRotationX(xangle));
                Gl.BindBufferToShaderAttribute(projections[i], program, "vertexPosition");
                Gl.BindBufferToShaderAttribute(cubeNormals, program, "vertexNormal");
                Gl.BindBufferToShaderAttribute(projectionsUV[i], program, "vertexUV");
                Gl.BindBuffer(cubeQuads);

                Gl.DrawElements(BeginMode.Quads, cubeQuads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);


            }

            //Gl.BindTexture(glassTexture2);
            //program["model_matrix"].SetValue(Matrix4.CreateRotationY(yangle+0.3f) * Matrix4.CreateRotationX(xangle));
            //Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
            //Gl.BindBufferToShaderAttribute(cubeNormals, program, "vertexNormal");
            //Gl.BindBufferToShaderAttribute(cubeUV, program, "vertexUV");
            //Gl.BindBuffer(cubeQuads);

            //Gl.DrawElements(BeginMode.Quads, cubeQuads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            Glut.glutSwapBuffers();

        }

        private static void OnReshape(int width, int height)
        {
            Program.width = width;
            Program.height = height;

            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
        }

        private static void OnKeyboardDown(byte key, int x, int y)
        {
            if (key == 'w') up = true;
            else if (key == 's') down = true;
            else if (key == 'd') right = true;
            else if (key == 'a') left = true;

            else if (key == 'y')
            {
                advance = true;
            }
            else if (key == 'k')
            {
                rotateMixA -= 0.1;
                if (rotateMixA < 0.0)
                {
                    rotateMixA = 0.0;
                }
                updateProjectionPlane();
                updateIntersectionInfo();
            }
            else if (key == 'i')
            {
                rotateMixA += 0.1;
                if (rotateMixA > 1.0)
                {
                    rotateMixA = 1.0;
                }
                updateProjectionPlane();
                updateIntersectionInfo();
            }
            else if (key == 'h')
            {
                retreat = true;
            }
            else if (key == 27) Glut.glutLeaveMainLoop();
        }

        private static void OnKeyboardUp(byte key, int x, int y)
        {
            if (key == 'w') up = false;
            else if (key == 's') down = false;
            else if (key == 'd') right = false;
            else if (key == 'a') left = false;
            else if (key == 'y')
            {
                advance = false;
            }
            else if (key == 'h')
            {
                retreat = false;
            }
            else if (key == ' ') autoRotate = !autoRotate;
            else if (key == 'l') lighting = !lighting;
            else if (key == 'f')
            {
                fullscreen = !fullscreen;
                if (fullscreen) Glut.glutFullScreen();
                else
                {
                    Glut.glutPositionWindow(0, 0);
                    Glut.glutReshapeWindow(1280, 720);
                }
            }
            else if (key == 'b')
            {
                alpha = !alpha;
                if (alpha)
                {
                    Gl.Enable(EnableCap.Blend);
                    Gl.Enable(EnableCap.ColorSum);
                    Gl.Disable(EnableCap.DepthTest);
                }
                else
                {
                    Gl.Disable(EnableCap.Blend);
                    Gl.Disable(EnableCap.ColorSum);
                    Gl.Enable(EnableCap.DepthTest);
                }
            }



        }


        public static string VertexShader = @"
        #version 130

                in vec3 vertexPosition;
                in vec3 vertexNormal;
                in vec4 vertexUV;

                out vec3 normal;
                out vec4 uv;

                uniform mat4 projection_matrix;
                uniform mat4 view_matrix;
                uniform mat4 model_matrix;

                void main(void)
                {
                    normal = normalize((model_matrix * vec4(floor(vertexNormal), 0)).xyz);
                    uv = vertexUV;

                    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
                }
                ";



        //        public static string VertexShader = @"
        //#version 130

        //        in vec3 vertexPosition;
        //        in vec3 vertexNormal;
        //        in vec2 vertexUV;

        //        out vec3 normal;
        //        out vec2 uv;

        //        uniform mat4 projection_matrix;
        //        uniform mat4 view_matrix;
        //        uniform mat4 model_matrix;

        //        void main(void)
        //        {
        //            normal = normalize((model_matrix * vec4(floor(vertexNormal), 0)).xyz);
        //            uv = vertexUV;

        //            gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
        //        }
        //        ";



        public static string FragmentShader = @"
        #version 130

        uniform sampler2D texture;
        uniform vec3 light_direction;
        uniform bool enable_lighting;

        in vec3 normal;
        in vec4 uv;

        out vec4 fragment;

        void main(void)
        {
            float diffuse = max(dot(normal, light_direction), 0);
            float ambient = 0.3;
            float lighting = (enable_lighting ? max(diffuse, ambient) : 1);

            // add in some blending for tutorial 8 by setting the alpha to 0.5
            fragment = vec4(lighting * texture2DProj(texture, uv).xyz, 1.0/360);
        }
        ";

        private static Texture glassTexture2;


        //        public static string FragmentShader = @"
        //#version 130

        //uniform sampler2D texture;
        //uniform vec3 light_direction;
        //uniform bool enable_lighting;

        //in vec3 normal;
        //in vec2 uv;

        //out vec4 fragment;

        //void main(void)
        //{
        //    float diffuse = max(dot(normal, light_direction), 0);
        //    float ambient = 0.3;
        //    float lighting = (enable_lighting ? max(diffuse, ambient) : 1);

        //    // add in some blending for tutorial 8 by setting the alpha to 0.5
        //    fragment = vec4(lighting* 4 * texture2D(texture, uv).xyz, 1.0/6);
        //}
        //";
    }



    class Vector3D
    {
        public double x { get; private set; }
        public double y { get; private set; }
        public double z { get; private set; }

        public Vector3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3D Subtract(Vector3D vector)
        {
            return new Vector3D(x - vector.x, y - vector.y, z - vector.z);
        }

        public Vector3D Add(Vector3D vector)
        {
            return new Vector3D(x + vector.x, y + vector.y, z + vector.z);
        }

        public double Abs()
        {
            double length2 = x * x + y * y + z * z;
            double length = Math.Sqrt(length2);
            return length;
        }

        public Vector3D Normalized()
        {
            double abs = Abs();
            return new Vector3D(x / abs, y / abs, z / abs);
        }

        public Vector3D Cross(Vector3D vector)
        {
            double newx = y * vector.z - z * vector.y;
            double newy = x * vector.z - z * vector.x;
            double newz = x * vector.y - y * vector.x;


            return new Vector3D(newx, newy, newz);
        }

        public double Dot(Vector3D vector)
        {
            return x * vector.x + y * vector.y + z * vector.z;
        }


        public Vector3D MultiplyScalar(double scalar)
        {
            return new Vector3D(this.x * scalar, this.y * scalar, this.z * scalar);
        }

    }


    class Plane3D
    {
        public Vector3D normal { get; private set; }
        public Vector3D offset { get; private set; }
        public Line3D orthoLine { get; private set; }

        public Vector3D lineIntersectAtSinglePoint(Line3D line)
        {

            if (!doesLineIntersect(line))
            {
                throw new Exception("Line has no intersection with this plane");
            }

            if (isLineParallel(line))
            {
                throw new Exception("Line is parallel to plane and intersects at all points along the line");
            }

            double d = offset.Subtract(line.offset).Dot(normal) / line.direction.Dot(normal);

            Vector3D intersection = line.direction.MultiplyScalar(d).Add(line.offset);

            return intersection;
        }

        public bool isLineParallel(Line3D line)
        {
            if (line.direction.Dot(normal) == 0)
            {
                return true;
            }
            return false;
        }

        public bool doesLineIntersectOnlyAtSinglePoint(Line3D line)
        {
            if (!doesLineIntersect(line))
            {
                return false;
            }

            if (isLineParallel(line))
            {
                return false;
            }

            return true;
        }

        public bool doesLineIntersectAtAllPoints(Line3D line)
        {
            if (!doesLineIntersect(line))
            {
                return false;
            }

            if (isLineParallel(line))
            {
                return true;
            }

            return false;

        }

        public bool doesLineIntersect(Line3D line)
        {
            if (isLineParallel(line))
            {
                if (offset.Subtract(line.offset).Dot(normal) != 0) // is line.offset in plane? if not then return false
                { 
                    return false;
                }
            }
            return true;
        }

        public Plane3D(Line3D orthogonalLine)
        {
            this.normal = orthogonalLine.direction;
            this.offset = orthogonalLine.offset;
            this.orthoLine = orthogonalLine;
        }

        public Plane3D(Vector3D normal, Vector3D offset)
        {
            this.normal = normal.Normalized();
            this.offset = offset;
            this.orthoLine = new Line3D(this.normal, this.offset);
        }
    }



    class Line3D
    {
        public Vector3D direction { get; private set; }
        public Vector3D offset { get; private set; }
        public Vector3D directionRaw { get; private set; }

        public Line3D(Vector3D direction, Vector3D offset)
        {
            setDirectionOffset(direction, offset);
        }

        public Line3D(Vector3D from, Vector3D to, int indicator)
        {
            setFromTo(from, to);
        }

        public Line3D()
        {
            this.direction = new Vector3D(0, 0, 0);
            this.offset = new Vector3D(0, 0, 0);
        }

        public void setFromTo(Vector3D from, Vector3D to)
        {
            Vector3D direction = to.Subtract(from);
            Vector3D offset = from;
            setDirectionOffset(direction, offset);
        }

        public void setDirectionOffset(Vector3D direction, Vector3D offset)
        {
            this.direction = direction.Normalized();
            this.directionRaw = direction;
            this.offset = offset;
        }
    }


    class DetectorObject
    {
        public Vector3D TopLeft { get; private set; }
        public Vector3D TopRight { get; private set; }
        public Vector3D BottomLeft { get; private set; }
        public Vector3D BottomRight { get; private set; }

        public DetectorObject(Vector3D TopLeft, Vector3D TopRight, Vector3D BottomLeft, Vector3D BottomRight)
        {
            this.TopLeft = TopLeft;
            this.TopRight = TopRight;
            this.BottomLeft = BottomLeft;
            this.BottomRight = BottomRight;
        }



    }


    class IntersectInfo
    {
        public Vector3D topLeftIntersect { get; private set; }
        public Vector3D bottomLeftIntersect { get; private set; }
        public Vector3D topRightIntersect { get; private set; }
        public Vector3D bottomRightIntersect { get; private set; }

        public Tuple<double, double> bottomLeftCrop { get; private set; }
        public Tuple<double, double> topLeftCrop { get; private set; }

        public Tuple<double, double> bottomRightCrop { get; private set; }
        public Tuple<double, double> topRightCrop { get; private set; }

        public IntersectInfo(Vector3D topLeftIntersect, Vector3D bottomLeftIntersect, Vector3D topRightIntersect, Vector3D bottomRightIntersect, Tuple<double, double> bottomLeftCrop, Tuple<double, double> topLeftCrop, Tuple<double, double> bottomRightCrop, Tuple<double, double> topRightCrop)
        {
            this.topLeftIntersect = topLeftIntersect;
            this.bottomLeftIntersect = bottomLeftIntersect;
            this.topRightIntersect = topRightIntersect;
            this.bottomRightIntersect = bottomRightIntersect;
            this.bottomLeftCrop = bottomLeftCrop;
            this.topLeftCrop = topLeftCrop;
            this.bottomRightCrop = bottomRightCrop;
            this.topRightCrop = topRightCrop;
        }
    }



    class InitProjections
    {


        public static double orthoProjectionCoefficient(Line3D line, Vector3D point)
        {
            Vector3D s = line.directionRaw;
            Vector3D v = point.Subtract(line.offset);
            double num = v.Dot(s);
            double den = s.Dot(s);
            return num / den;
        }

        public static Vector3D orthoProjection(Line3D line, Vector3D point)
        {
            Vector3D s = line.directionRaw;
            //Vector3D v = point.Subtract(line.offset);
            //double num = v.Dot(s);
            //double den = s.Dot(s);
            return s.MultiplyScalar(orthoProjectionCoefficient(line, point));
        }

        public static IntersectInfo calculateTextureProjection(Vector3D sourceVector, DetectorObject detector, Plane3D projectionPlane)
        {
            //Vector3D sourceVector = new Vector3D(-1, 0, 0.5);


            //DetectorObject detector =
            //    new DetectorObject(
            //        new Vector3D(1, -0.5, 1),
            //        new Vector3D(1, 0.5, 1),
            //        new Vector3D(1, -0.5, 0),
            //        new Vector3D(1, 0.5, 0)
            //    );


            //Plane3D projectionPlane = new Plane3D(new Line3D(new Vector3D(0, 0, 1), new Vector3D(0, 0, 0.1)));

            //var planeUV = new Tuple<double, double>[]({ new Tuple<double, double>(0, 0), new Tuple<double, double>(1, 0), new Tuple<double, double>(1, 1), new Tuple<double, double>(0, 1)});

            Vector3D topLeftIntersect = detector.TopLeft;
            Vector3D bottomLeftIntersect = detector.BottomLeft;
            Vector3D topRightIntersect = detector.TopRight;
            Vector3D bottomRightIntersect = detector.BottomRight;

            Tuple<double, double> bottomLeftCrop = new Tuple<double, double>(0, 0);
            Tuple<double, double> topLeftCrop = new Tuple<double, double>(0, 1);

            Tuple<double, double> bottomRightCrop = new Tuple<double, double>(1, 0);
            Tuple<double, double> topRightCrop = new Tuple<double, double>(1, 1);

            double sourceProjectionCoefficientRaw = orthoProjectionCoefficient(projectionPlane.orthoLine, sourceVector);
            double sourceProjectionCoefficient = orthoProjection(projectionPlane.orthoLine, sourceVector).Abs() * sourceProjectionCoefficientRaw / Math.Abs(sourceProjectionCoefficientRaw);


            //Find texture projection onto projectionPlane
            Line3D DetectorLineLeftBottomTop = new Line3D(detector.BottomLeft, detector.TopLeft, 0);

            if (projectionPlane.doesLineIntersect(DetectorLineLeftBottomTop))
            {
                if (projectionPlane.doesLineIntersectOnlyAtSinglePoint(DetectorLineLeftBottomTop))
                {
                    Vector3D intersectLeftBottomTop = projectionPlane.lineIntersectAtSinglePoint(DetectorLineLeftBottomTop);
                    double intersectCoefficientRaw = orthoProjectionCoefficient(DetectorLineLeftBottomTop, intersectLeftBottomTop);
                    double intersectCoefficient = orthoProjection(DetectorLineLeftBottomTop, intersectLeftBottomTop).Abs() * intersectCoefficientRaw / (Math.Abs(intersectCoefficientRaw) * DetectorLineLeftBottomTop.directionRaw.Abs());
                    if (intersectCoefficient < 0 || intersectCoefficient > 1)
                    {
                        //Program.mw?.write("Left: No panel intersection.");
                        //Debug.WriteLine("Left: No panel intersection.");
                        //It doesn't intersect with the panel before the projectionPlane..
                        //therefore crop/UV vectors for left top/bottom are unity

                        //Calc bottomLeft and topLeft intersections
                        Line3D topLeftLine3D = new Line3D(sourceVector, detector.TopLeft, 0);
                        Line3D bottomLeftLine3D = new Line3D(sourceVector, detector.BottomLeft, 0);
                        //if (projectionPlane.doesLineIntersectOnlyAtSinglePoint(topLeftLine3D))
                        //{
                            topLeftIntersect = projectionPlane.lineIntersectAtSinglePoint(topLeftLine3D);
                        //}
                        //else
                        //{
                        //    if (projectionPlane.doesLineIntersect(topLeftLine3D))
                        //    {
                        //        topLeftIntersect = 
                        //    }
                        //    else
                        //    {
                        //        throw new Exception("Line does not intersect with plane!");
                        //    }
                        //}
                        //if (projectionPlane.doesLineIntersectOnlyAtSinglePoint(bottomLeftLine3D))
                        //{
                            bottomLeftIntersect = projectionPlane.lineIntersectAtSinglePoint(bottomLeftLine3D);
                        //}


                        //topRightIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.TopRight,0));
                        //bottomRightIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.BottomRight,0));

                    }
                    else
                    {
                        if (sourceProjectionCoefficient < 0)
                        {
                            //Program.mw?.write("Left: Panel intersection, source is below.");
                            bottomLeftCrop = new Tuple<double, double>(topLeftCrop.Item1, intersectCoefficient);
                            bottomLeftIntersect = intersectLeftBottomTop;

                            //Calc topLeft intersection
                            topLeftIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.TopLeft, 0));
                        }
                        else if (sourceProjectionCoefficient > 0)
                        {
                            //Program.mw?.write("Left: Panel intersection, source is above.");
                            topLeftCrop = new Tuple<double, double>(bottomLeftCrop.Item1, intersectCoefficient);
                            topLeftIntersect = intersectLeftBottomTop;

                            //Calc bottomLeft intersection
                            bottomLeftIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.BottomLeft, 0));
                        }
                    }

                }
            }



            //Find texture projection onto projectionPlane
            Line3D DetectorLineRightBottomTop = new Line3D(detector.BottomRight, detector.TopRight, 0);

            if (projectionPlane.doesLineIntersect(DetectorLineRightBottomTop))
            {
                if (projectionPlane.doesLineIntersectOnlyAtSinglePoint(DetectorLineRightBottomTop))
                {
                    Vector3D intersectRightBottomTop = projectionPlane.lineIntersectAtSinglePoint(DetectorLineRightBottomTop);
                    double intersectCoefficientRaw = orthoProjectionCoefficient(DetectorLineRightBottomTop, intersectRightBottomTop);
                    double intersectCoefficient = orthoProjection(DetectorLineRightBottomTop, intersectRightBottomTop).Abs() * intersectCoefficientRaw / (Math.Abs(intersectCoefficientRaw) * DetectorLineRightBottomTop.directionRaw.Abs());
                    if (intersectCoefficient < 0 || intersectCoefficient > 1)
                    {

                        //Program.mw?.write("Right: No panel intersection.");
                        //It doesn't intersect with the panel before the projectionPlane..
                        //therefore crop/UV vectors for left top/bottom are unity

                        //Calc bottomLeft and topLeft intersections
                        //topLeftIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.TopLeft,0));
                        //bottomLeftIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.BottomLeft,0));


                        topRightIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.TopRight, 0));
                        bottomRightIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.BottomRight, 0));

                    }
                    else
                    {
                        if (sourceProjectionCoefficient < 0)
                        {
                            //Program.mw?.write("Right: Panel intersection, source is below.");
                            bottomRightCrop = new Tuple<double, double>(topRightCrop.Item1, intersectCoefficient);
                            bottomRightIntersect = intersectRightBottomTop;

                            //Calc topRight intersection
                            topRightIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.TopRight, 0));
                        }
                        else if (sourceProjectionCoefficient > 0)
                        {
                            //Program.mw?.write("Right: Panel intersection, source is above.");
                            topRightCrop = new Tuple<double, double>(bottomRightCrop.Item1, intersectCoefficient);
                            topRightIntersect = intersectRightBottomTop;

                            //Calc bottomRight intersection
                            bottomRightIntersect = projectionPlane.lineIntersectAtSinglePoint(new Line3D(sourceVector, detector.BottomRight, 0));
                        }
                    }

                }
            }







            //Line3D DetectorLineBottomLeftRight = new Line3D(detector.BottomLeft, detector.BottomRight, 0); //For later use/implementation
            //Line3D DetectorLineTopLeftRight = new Line3D(detector.TopLeft, detector.TopRight, 0);//For later use/implementation


            IntersectInfo retval = new IntersectInfo(topLeftIntersect, bottomLeftIntersect, topRightIntersect, bottomRightIntersect, bottomLeftCrop, topLeftCrop, bottomRightCrop, topRightCrop);

            return retval;
        }
    }
}
