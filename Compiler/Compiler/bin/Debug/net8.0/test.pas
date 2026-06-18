program TestOK(input, output);

const
    max = 100;
    pi = 3.14;

type
    TPerson = record
        name: string;
        age: integer;
    end;

var
    a, b: integer;
    c: real;
    d: char;
    arr: array[1..10] of integer;
    person: TPerson;

begin
    a := 10;
    b := 20;
    c := a + b * 2.5;
    d := 'A';

    arr[1] := 5;
    arr[a] := b;

    person.name := 'John';
    person.age := 30;

    with person do
    begin
        name := 'Jane';
        age := age + 1;
    end;
end.