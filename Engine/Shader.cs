using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace Engine
{
    public class Shader
    {
        private readonly int handle;
        private readonly int vertexHandle;
        private readonly int fragmentHandle;

        public Shader(string vertexFileName, string fragmentFileName)
        {
            handle = GL.CreateProgram();
            vertexHandle = LoadShader(vertexFileName, ShaderType.VertexShader);
            fragmentHandle = LoadShader(fragmentFileName, ShaderType.FragmentShader);
            //BindAttributes();
            GL.LinkProgram(handle);
            GL.ValidateProgram(handle);
            GL.DeleteShader(vertexHandle);
            GL.DeleteShader(fragmentHandle);
            //GetAllUniformLocations();
        }

        private int LoadShader(string fileName, ShaderType type)
        {
            int shaderHandle = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader($"Shaders/{fileName}"))
                GL.ShaderSource(shaderHandle, sr.ReadToEnd());
            GL.CompileShader(shaderHandle);

            string infoLogShader = GL.GetShaderInfoLog(shaderHandle);
            if (infoLogShader != string.Empty) throw new Exception(infoLogShader);
            GL.AttachShader(handle, shaderHandle);

            return shaderHandle;
        }

        public void SetStart()
        {
            GL.UseProgram(handle);
        }

        public void SetStop()
        {
            GL.UseProgram(0);
        }

        public void SetUniform(int location, bool value)
        {
            if (value)
            {
                GL.Uniform1(location, 1.0f);
            }
            else
            {
                GL.Uniform1(location, 0.0f);
            }
        }
        public void SetUniform(int location, int value)
        {
            GL.Uniform1(location, value);
        }
        public void SetUniform(int location, float value)
        {
            GL.Uniform1(location, value);
        }
        public void SetUniform(int location, Vector2 value)
        {
            GL.Uniform2(location, value);
        }
        public void SetUniform(int location, Vector3 value)
        {
            GL.Uniform3(location, value);
        }
        public void SetUniform(int location, Vector4 value)
        {
            GL.Uniform4(location, value);
        }
        public void SetUniform(int location, Matrix4 value)
        {
            GL.UniformMatrix4(location, false, ref value);
        }

        public void SetUniform(string name, bool value)
        {

            if (value)
            {
                GL.Uniform1(GL.GetUniformLocation(handle, name), 1.0f);
            }
            else
            {
                GL.Uniform1(GL.GetUniformLocation(handle, name), 0.0f);
            }
        }
        public void SetUniform(string name, int value)
        {
            GL.Uniform1(GL.GetUniformLocation(handle, name), value);
        }
        public void SetUniform(string name, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(handle, name), value);
        }
        public void SetUniform(string name, Vector2 value)
        {
            GL.Uniform2(GL.GetUniformLocation(handle, name), value);
        }
        public void SetUniform(string name, Vector3 value)
        {
            GL.Uniform3(GL.GetUniformLocation(handle, name), value);
        }
        public void SetUniform(string name, Vector4 value)
        {
            GL.Uniform4(GL.GetUniformLocation(handle, name), value);
        }
        public void SetUniform(string name, Matrix4 value)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(handle, name), false, ref value);
        }

        protected int GetUniformLocation(string uniformName)
        {
            return GL.GetUniformLocation(handle, uniformName);
        }

        protected void BindAttribute(int attribute, string name)
        {
            GL.BindAttribLocation(handle, attribute, name);
        }

        public void Delete()
        {
            SetStop();
            GL.DetachShader(handle, vertexHandle);
            GL.DetachShader(handle, fragmentHandle);
            GL.DeleteProgram(handle);
        }

        //protected abstract void GetAllUniformLocations();

        //protected abstract void BindAttributes();

    }

}
