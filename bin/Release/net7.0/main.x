// Hello World, You will never read this lol

printLn("Hello");

assign mut expression = "test";
assign mut array = [
    "hello",
    "world",
    "test",
];

assign mut count = 0;
for i = 0 : 10 then
    set count = count + 1;
    printLn("Hello");
end


switch expression then
    default:
        printLn("Correct!");
        break;
    case "hello":
        printLn("Hello!");
        break;
    case "hi":
        printLn("Hello!");
        break;
end
printLn(array);