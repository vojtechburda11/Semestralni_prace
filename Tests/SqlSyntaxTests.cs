using Microsoft.SqlServer.TransactSql.ScriptDom;
using Xunit;
public class SqlSyntaxTests {
    [Theory]
    [InlineData("01_Schema.sql")]
    [InlineData("02_Dotazy.sql")]
    [InlineData("03_UkazkovaData.sql")]
    public void SkriptyATriggerMajiPlatnouSyntaxiSqlServer2022(string name) {
        var parser = new TSql160Parser(true);
        var text = File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"sql",name));
        var tree = parser.Parse(new StringReader(text),out var errors);
        Assert.True(errors.Count == 0,string.Join("\n", errors.Select(x=>$"Řádek {x.Line}: {x.Message}")));
        tree.Accept(new TriggerVisitor());
    }
    private sealed class TriggerVisitor : TSqlFragmentVisitor {
        public override void ExplicitVisit(StringLiteral node) {
            if (!node.Value.TrimStart().StartsWith("CREATE OR ALTER TRIGGER")) return;
            new TSql160Parser(true).Parse(new StringReader(node.Value),out var errors);
            Assert.True(errors.Count == 0,string.Join("\n",errors.Select(x=>x.Message)));
        }
    }
}
