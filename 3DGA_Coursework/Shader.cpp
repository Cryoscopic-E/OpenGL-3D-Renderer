#include "Shader.h"

//TODO cite https://community.khronos.org/t/basic-opengl-shader-class-codereview/75726
Shader::Shader(const char* vertex, const char* fragment)
{
	this->fsPath = fragment;
	this->vsPath = vertex;
	loadShaders();
}

Shader::~Shader()
{
	glDeleteProgram(this->program);
}

void Shader::bind()
{
	glUseProgram(this->program);
}

void Shader::unbind()
{
	glUseProgram(0);
}

void Shader::setUniform(const char* name, bool val)
{
	glUniform1i(getUniformLocation(name), val);
}

void Shader::setUniform(const char* name, int val)
{
	glUniform1i(getUniformLocation(name), val);
}

void Shader::setUniform(const char* name, float val)
{
	glUniform1f(getUniformLocation(name), val);
}

void Shader::setUniform(const char* name, glm::vec3 val)
{
	glUniform3f(getUniformLocation(name), val.x, val.y, val.z);
}

void Shader::setUniform(const char* name, glm::mat4 val)
{
	glUniformMatrix4fv(getUniformLocation(name), 1, GL_FALSE, &val[0][0]);
}

void Shader::loadShaders()
{

	std::ifstream vsFile;
	std::ifstream fsFile;

	std::stringstream fsSS;
	std::stringstream vsSS;

	std::string vsFileContent;
	std::string fsFileContent;

	vsFile.exceptions(std::ifstream::failbit | std::ifstream::badbit);
	fsFile.exceptions(std::ifstream::failbit | std::ifstream::badbit);

	try
	{
		fsFile.open(this->fsPath);
		fsSS << fsFile.rdbuf();
		fsFileContent = fsSS.str();
		fsFile.close();

		vsFile.open(this->vsPath);
		vsSS << vsFile.rdbuf();
		vsFileContent = vsSS.str();
		vsFile.close();
	}
	catch (std::ifstream::failure e)
	{
		std::cout << "Error reading shaders" << std::endl;
	}

	this->fsCode = fsFileContent.c_str();
	this->vsCode = vsFileContent.c_str();

	GLuint vertexShader = glCreateShader(GL_VERTEX_SHADER);
	glShaderSource(vertexShader, 1, &this->vsCode, NULL);
	glCompileShader(vertexShader);
	checkCompilationError(vertexShader);

	GLuint fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
	glShaderSource(fragmentShader, 1, &this->fsCode, NULL);
	glCompileShader(fragmentShader);
	checkCompilationError(fragmentShader);

	this->program = glCreateProgram();
	glAttachShader(this->program, vertexShader);
	glAttachShader(this->program, fragmentShader);
	glLinkProgram(this->program);
	checkProgramLinkError();

	glDeleteShader(vertexShader);
	glDeleteShader(fragmentShader);
}

void Shader::checkCompilationError(GLuint shader)
{
	int ok;
	glGetShaderiv(shader, GL_COMPILE_STATUS, &ok);
	if (!ok)
	{
		glGetShaderInfoLog(shader, 512,NULL, this->log);
		std::cout << "error compiling shader " << this->log << std::endl;
	}
}

void Shader::checkProgramLinkError()
{
	int ok;
	glGetProgramiv(program, GL_LINK_STATUS, &ok);
	if (!ok)
	{
		glGetProgramInfoLog(program, 512, NULL, this->log);
		std::cout << "error linking shader program " << this->log << std::endl;
	}
}

GLint Shader::getUniformLocation(const char* name)
{
	return glGetUniformLocation(this->program, name);
}
