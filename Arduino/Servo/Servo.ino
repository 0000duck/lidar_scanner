#include <Servo.h>

Servo ServoMotor;

void setup()
{
  ServoMotor.attach(9);
}

void loop()
{
  for (int i = 0; i <= 180; i += 1)
  {
    ServoMotor.write(i);
    
    delay(25);
  }
  
  for (int i = 180; i >= 0; i -= 1)
  {
    ServoMotor.write(i);
    
    delay(25);
  }
}
