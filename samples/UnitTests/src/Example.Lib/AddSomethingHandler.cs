namespace Example.Lib;

public class AddSomethingHandler
{
    private readonly ISomeService someService;

    public AddSomethingHandler(ISomeService someService) 
        => this.someService = someService;

    public int Handle(AddSomething request)
        => someService.Calculate(request.Number);
}