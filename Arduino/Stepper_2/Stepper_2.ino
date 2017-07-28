#define IN1  8
#define IN2  9
#define IN3  10
#define IN4  11

const int MinimumCase = 0,
          MaximumCase = 7;

boolean Direction;
int Steps;

void setup()
{
  Serial.begin(9600);
  
  pinMode(IN1, OUTPUT);
  pinMode(IN2, OUTPUT);
  pinMode(IN3, OUTPUT);
  pinMode(IN4, OUTPUT);

  Initialise();
}

void Initialise()
{
  Direction = false;
  
  Steps = 0;
}

void loop()
{
  while(true)
  {
    if(!Direction)
    {
      if(SerialRead() == 'A')
      {
        Direction = !Direction;
      }
    }
    else
    {
      delay(1);
    }
    
    Move();

    if(!Direction)
    {
      Serial.print("A");
    }
  }
}

char SerialRead()
{
  while(!Serial.available())
  {
    delay(25);
  }

  delay(25);
  
  return Serial.read();
}

void Move()
{
  switch(Steps)
  {
     case 0:
     
       digitalWrite(IN1, LOW); 
       digitalWrite(IN2, LOW);
       digitalWrite(IN3, LOW);
       digitalWrite(IN4, HIGH);
       
     break;
     
     case 1:
     
       digitalWrite(IN1, LOW); 
       digitalWrite(IN2, LOW);
       digitalWrite(IN3, HIGH);
       digitalWrite(IN4, HIGH);
       
     break;
     
     case 2:
     
       digitalWrite(IN1, LOW); 
       digitalWrite(IN2, LOW);
       digitalWrite(IN3, HIGH);
       digitalWrite(IN4, LOW);
       
     break;
     
     case 3:
     
       digitalWrite(IN1, LOW); 
       digitalWrite(IN2, HIGH);
       digitalWrite(IN3, HIGH);
       digitalWrite(IN4, LOW);
       
     break;
     
     case 4:
     
       digitalWrite(IN1, LOW); 
       digitalWrite(IN2, HIGH);
       digitalWrite(IN3, LOW);
       digitalWrite(IN4, LOW);
       
     break;
     
     case 5:
     
       digitalWrite(IN1, HIGH); 
       digitalWrite(IN2, HIGH);
       digitalWrite(IN3, LOW);
       digitalWrite(IN4, LOW);
       
     break;
     
     case 6:
     
       digitalWrite(IN1, HIGH); 
       digitalWrite(IN2, LOW);
       digitalWrite(IN3, LOW);
       digitalWrite(IN4, LOW);
       
     break;
     
     case 7:
     
       digitalWrite(IN1, HIGH); 
       digitalWrite(IN2, LOW);
       digitalWrite(IN3, LOW);
       digitalWrite(IN4, HIGH);
       
     break;
     
     default:
     
       digitalWrite(IN1, LOW); 
       digitalWrite(IN2, LOW);
       digitalWrite(IN3, LOW);
       digitalWrite(IN4, LOW);
       
     break; 
  }
  
  SetSteps();
}

void SetSteps()
{
  if(Direction)
  {
    Steps++;
  }
  else
  {
    if(!Direction)
    {
      Steps--;
    }
  }
  
  if(Steps > MaximumCase)
  {
    Steps = MinimumCase;
  }
  else
  {
    if(Steps < MinimumCase)
    {
      Steps = MaximumCase;
    }
  }
}
