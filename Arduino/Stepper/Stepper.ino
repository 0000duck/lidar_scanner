#include <Stepper.h>

const int steps = 200;

Stepper stepper(steps, 8, 9, 10, 11);

void setup()
{
  stepper.setSpeed(60);
}

void loop()
{
  stepper.step(steps);
  stepper.step(-steps);
}
