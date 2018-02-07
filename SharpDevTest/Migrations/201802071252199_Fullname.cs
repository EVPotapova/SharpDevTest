namespace SharpDevTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fullname : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "FullName", c => c.String(nullable: false));
            DropColumn("dbo.AspNetUsers", "FirstName");
            DropColumn("dbo.AspNetUsers", "LastName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "LastName", c => c.String(nullable: false));
            AddColumn("dbo.AspNetUsers", "FirstName", c => c.String(nullable: false));
            DropColumn("dbo.AspNetUsers", "FullName");
        }
    }
}
