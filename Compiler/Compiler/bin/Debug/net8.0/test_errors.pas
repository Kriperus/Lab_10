program TestErrors(input, output);

const
    c = 3;
    b = 56;

type
    TPerson = record
        name: string;
        age: integer;
    end;

var
    a, b, k, i: integer;
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

    arr[1] := 'A';
    person.age := 'B';
    x := 100;
    person.surname := 'Smith';
    arr2[1] := 10;
end.