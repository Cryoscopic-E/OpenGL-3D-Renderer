#version 430 core

out vec4 color;

in VS_OUT
{
  vec2 tc;
  vec3 normals;
  vec3 fragPos;
}fs_in;


struct DirectionalLight{
  vec3 direction;
  vec3 ambient;
  vec3 diffuse;
  vec3 specular;
};

struct PointLight{
  vec3 position;
  vec3 ambient;
  vec3 diffuse;
  vec3 specular;
  float constant;
  float linear;
  float quadratic;
};

struct SpotLight{
  vec3 position;
  vec3 direction;
  vec3 ambient;
  vec3 diffuse;
  vec3 specular;
  float constant;
  float linear;
  float quadratic;
  float cutOff;
  float outCutOff;
};


struct Material{
  float shininess;
  sampler2D textureDiffuse;
  sampler2D textureSpecular;
};

#define N_SPOT_LIGHTS 3
#define N_POINT_LIGHTS 2

uniform vec3 viewPosition;
uniform DirectionalLight directionalLight;
uniform PointLight pointLights[N_POINT_LIGHTS];
uniform SpotLight spotLights[N_SPOT_LIGHTS];
uniform Material material;

vec3 ContributeDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDirection);
vec3 ContributePointLight(PointLight light, vec3 normal,vec3 fragmentPosition, vec3 viewDirection);
vec3 ContributeSpotLight(SpotLight light, vec3 normal,vec3 fragmentPosition, vec3 viewDirection);

void main(void){

  //prop
  vec3 norm = normalize(fs_in.normals);
  vec3 viewDirection = normalize(viewPosition - fs_in.fragPos);

  // calculate contribution from directional light
  vec3 result = ContributeDirectionalLight(directionalLight,norm,viewDirection);
  //calculate contribution from point lights
  for(int i=0; i< N_POINT_LIGHTS; i++){
    result += ContributePointLight(pointLights[i], norm,fs_in.fragPos, viewDirection);
  }
  // // calculate contributions from spotlights
  for(int i=0; i< N_SPOT_LIGHTS; i++){
    result += ContributeSpotLight(spotLights[i], norm,fs_in.fragPos, viewDirection);
  }
  color=vec4(result,1.f);
}

vec3 ContributeDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDirection)
{
  vec3 lightDirection = normalize(-light.direction);
  //diffuse
  float diff = max(dot(normal, lightDirection), 0.0);
  //specular
  vec3 reflectionDirection = reflect(-lightDirection,normal);
  float specularReflection = pow(max(dot(viewDirection,reflectionDirection),0.0), material.shininess);
  //result
  vec3 ambient = light.ambient * vec3(texture(material.textureDiffuse, fs_in.tc));
  vec3 diffuse = light.diffuse * diff * vec3(texture(material.textureDiffuse, fs_in.tc));
  vec3 specular = light.specular *specularReflection * vec3(texture(material.textureSpecular, fs_in.tc));
  
  return (ambient + diffuse + specular);

}

vec3 ContributePointLight(PointLight light, vec3 normal,vec3 fragmentPosition, vec3 viewDirection)
{
  vec3 lightDirection = normalize(light.position - fs_in.fragPos);
  //diffuse
  float diff = max(dot(normal, lightDirection), 0.0);
  //specular
  vec3 reflectionDirection = reflect(-lightDirection,normal);
  float specularReflection = pow(max(dot(viewDirection,reflectionDirection),0.0), material.shininess);
  // attenuation
  float distance = length(light.position - fragmentPosition);
  float attenuation = 1.0/(light.constant + light.linear * distance + light.quadratic * (distance * distance));
  
  //result
  vec3 ambient = light.ambient * vec3(texture(material.textureDiffuse, fs_in.tc));
  vec3 diffuse = light.diffuse * diff * vec3(texture(material.textureDiffuse, fs_in.tc));
  vec3 specular = light.specular *specularReflection * vec3(texture(material.textureSpecular, fs_in.tc));

  ambient*= attenuation;
  diffuse*= attenuation;
  specular *= attenuation;

  return (ambient + diffuse + specular);
}

vec3 ContributeSpotLight(SpotLight light, vec3 normal,vec3 fragmentPosition, vec3 viewDirection)
{
  vec3 lightDirection = normalize(-light.direction);
  //diffuse
  float diff = max(dot(normal, lightDirection), 0.0);
  //specular
  vec3 reflectionDirection = reflect(-lightDirection,normal);
  float specularReflection = pow(max(dot(viewDirection,reflectionDirection),0.0), material.shininess);
  // attenuation
  float distance = length(light.position - fragmentPosition);
  float attenuation = 1.0/(light.constant + light.linear * distance + light.quadratic * (distance * distance));
  // spotlight
  float theta = dot(lightDirection, normalize(-light.direction));
  float epsilon = light.cutOff - light.outCutOff;
  float intensity = clamp((theta - light.outCutOff)/epsilon,0.0,1.0);
  //result
  vec3 ambient = light.ambient * vec3(texture(material.textureDiffuse, fs_in.tc));
  vec3 diffuse = light.diffuse * diff * vec3(texture(material.textureDiffuse, fs_in.tc));
  vec3 specular = light.specular *specularReflection * vec3(texture(material.textureSpecular, fs_in.tc));

  ambient*= attenuation * intensity;
  diffuse*= attenuation * intensity;
  specular *= attenuation * intensity;

  return (ambient + diffuse + specular);
}