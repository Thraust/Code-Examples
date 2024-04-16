import discord
import random
import pyodbc
from discord.ext import commands
from .sql import SQL

class Moderation(commands.Cog):

    def __init__(self, client):
        self.client = client

    # Events
    @commands.Cog.listener()
    async def on_ready(self):
        print('Moderation Ready')

    @commands.Cog.listener()
    async def on_member_join(self, member):
        print(f'{member} has joined a server.')
    
    @commands.Cog.listener()
    async def on_member_remove(self, member):
        print(f'{member} was kicked from the server.')
            
    # Commands
    @commands.command()
    async def ping(self, ctx):
        await ctx.send(f'Pong! {round(self.client.latency * 1000)}ms')

    @commands.command()
    # @commands.has_permissions(manage_messages=True)
    async def clear(self, ctx, amount:int):
        #if ctx.author.id == <ID Number>: # 
        #    responses = ('No.','Not today.',"I don't feel like it.","You broke me before, I'm not going through that again")
        #    await ctx.send(f'{random.choice(responses)}')
        #el
        if ctx.guild.id == <ID Number> and self.<ServerName>(ctx):
            await ctx.channel.purge(limit=amount + 1)
        elif ctx.guild.id != <ID Number>:
            await ctx.channel.purge(limit=amount + 1)
        else:
            await ctx.send(f"NICE TRY")
    
    @commands.command()
    async def newclear(self, ctx, amount:int):
        messages = await ctx.channel.history(limit=amount + 1).flatten()
        for i in messages:
            sqlcur = SQL.opencur(ctx)
            sqlcur.execute(f"insert into [dbo].[deleted_messages](author,discord,channel,deleted_at,message_text) values('{i.author}','{ctx.guild.name}','{ctx.channel.name}',getdate(),'{i.content}')")
            sqlcur.commit()
            sqlcur.close()
        await ctx.channel.purge(limit=amount + 1)
    
    def is_it_me(ctx):
        return ctx.author.id == <ID Number>

    def discordServerName(self,ctx):
        if ctx.author.id == <ID Number>:
            return ctx.author.id == <ID Number>
        elif ctx.author.id == <ID Number>:
            return ctx.author.id == <ID Number>
    
    @commands.command()
    @commands.check(is_it_me)
    async def nyxcreator(self, ctx):
        await ctx.send(f'Bot Author Confirmed: {ctx.author}')
    
    @commands.command()
    @commands.has_permissions(kick_members=True)
    async def kick(self, ctx, member : discord.Member, *, reason=None):
        await member.kick(reason=reason)
        await ctx.send(f'Kicked {member.mention}')

    @commands.command()
    @commands.has_permissions(ban_members=True)
    async def ban(self, ctx, member : discord.Member, *, reason=None):
        await member.ban(reason=reason)
        await ctx.send(f'Banned {member.mention}')

    @commands.command()
    @commands.has_permissions(ban_members=True)
    async def unban(self, ctx, *, member):
        banned_users = await ctx.guild.bans()
        member_name, member_discriminator = member.split('#')

        for ban_entry in banned_users:
            user = ban_entry.user

            if (user.name, user.discriminator) == (member_name, member_discriminator):
                await ctx.guild.unban(user)
                await ctx.send(f'Unbanned {user.mention}')
                return
    
    # Error Handling
    @clear.error
    async def clear_error(self, ctx, error):
        if isinstance(error, commands.MissingRequiredArgument):
            await ctx.send('Missing argument')
        elif isinstance(error, commands.BadArgument):
            await ctx.send('Looking for a number...')
        elif isinstance(error, commands.UserInputError):
            await ctx.send('stuff')
        else:
            print(error)
            await ctx.send('Something went wrong... obvs!')
            await ctx.send(error)
            
    @newclear.error
    async def clear_error(self, ctx, error):
        print(error)
        await ctx.send('Something went wrong... obvs!')
        await ctx.send(error)
        
def setup(client):
    client.add_cog(Moderation(client))