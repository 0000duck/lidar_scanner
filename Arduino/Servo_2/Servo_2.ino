#include <Servo.h>

const int Delay = 25;

static Servo ServoMotor;

void setup()
{
  Serial.begin(9600);

  Initialise();
}

void Initialise()
{
  MoveServo(0);
  
  Serial.print('S');
}

void MoveServo(int angle)
{
  ServoMotor.attach(9);

  delay(Delay);

  int currentAngle = ServoMotor.read();
  
  if(currentAngle < angle)
  {
    for(int i = currentAngle; i < angle + 1; i++)
    {
      ServoMotor.write(i);
  
      delay(Delay);
    }
  }
  else
  {
    for(int i = currentAngle; i > angle - 1; i--)
    {
      ServoMotor.write(i);
  
      delay(Delay);
    }
  }

  delay(Delay);
  
  ServoMotor.detach();
}

void loop()
{
  while(!Serial.available())
  {
    delay(Delay);
  }
  
  delay(Delay);

  char serialMessage = Serial.read();

  if(serialMessage == 'A')
  {
    int intLength = (int)Serial.read();
      
    char intCharArray[intLength];
      
    for(int i = 0; i < intLength; i++)
    {
      intCharArray[i] = Serial.read();
    }

    MoveServo(atoi(intCharArray));
  }
  
  while(Serial.available())
  {
    Serial.read();
  }

  Serial.print('E');
}

