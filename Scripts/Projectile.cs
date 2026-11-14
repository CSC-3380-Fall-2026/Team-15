using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Projectile : Area2D
{
    public Vector2 Direction = Vector2.Zero;
    [Export] public float Speed = 400f;
    [Export] public int Damage = 10;

    public override void _PhysicsProcess(double delta)
    {
        Position += Direction * Speed * (float)delta;
    }

    public void _on_body_entered(Node2D body)
    {
        //ignores other enemies 
        if(body is Enemy)
        {
            return;
        }

        // Applies damage if hits player
        if (body.IsInGroup("Player") && body is Player player)
        {
            player.TakeDamage(Damage);
            QueueFree();
        }
        else
        {
            // Destroys projectile if it hits wall/environment
            QueueFree();
        }
    }

    

}
